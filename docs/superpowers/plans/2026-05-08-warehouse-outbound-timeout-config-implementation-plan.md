# Warehouse Outbound Timeout Config Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add warehouse-level outbound-timeout configuration, a shared deadline calculator, and wire the settings/anomaly/dashboard flows to one shared outbound timing rule source.

**Architecture:** Reuse the existing `settings/alerts` contract instead of adding a new settings endpoint. Move the in-memory settings payload into a shared singleton store keyed by warehouse, map saved alert settings to a `WarehouseOutboundRuleConfig`, and have outbound anomaly/dashboard responses consume that rule through one shared calculator. Keep `leadTimes.outbound` as the warning lead value for this MVP, expressed in hours, and expose it in the settings page as the outbound warning lead field.

**Tech Stack:** ASP.NET Core Web API, xUnit, Vue 3, TypeScript, Element Plus

---

## File Map

- Modify: `backend/Contracts/Settings/SettingsDtos.cs`
- Modify: `backend/Api/Controllers/SettingsController.cs`
- Modify: `backend/Api/Controllers/AnomaliesController.cs`
- Modify: `backend/Api/Controllers/DashboardController.cs`
- Modify: `backend/Api/Program.cs`
- Create: `backend/Application/Settings/AlertsSettingsStore.cs`
- Create: `backend/Application/Outbound/WarehouseOutboundRuleConfig.cs`
- Create: `backend/Application/Outbound/WarehouseOutboundRuleProvider.cs`
- Create: `backend/Application/Outbound/WarehouseBusinessCalendarService.cs`
- Create: `backend/Application/Outbound/OutboundDeadlineCalculator.cs`
- Create: `backend/Api.Tests/SettingsControllerTests.cs`
- Create: `backend/Api.Tests/OutboundDeadlineCalculatorTests.cs`
- Modify: `backend/Api.Tests/AnomaliesControllerTests.cs`
- Modify: `backend/Api.Tests/DashboardControllerTests.cs`
- Modify: `frontend/src/services/types.ts`
- Modify: `frontend/src/modules/settings/views/AlertsConfigView.vue`
- Optional follow-up if the page later adds warehouse switching: `frontend/src/services/api.ts`

## Delivery Notes

- `leadTimes.outbound` is the source of truth for outbound warning lead in this MVP. Do not add `warningLeadMinutes` to `OutboundRuleConfig`.
- Backend settings storage must be keyed by `warehouseId` so `warehouseCode=DE` and `warehouseCode=ON` can read different rules.
- `GET /api/settings/alerts` may accept an optional `warehouseId` query parameter and should default to `DE`. The current frontend keeps its existing single-warehouse behavior and does not add a warehouse switch in this plan.

### Task 1: Expand The Settings Contract And Move It Into A Shared Store

**Files:**
- Modify: `backend/Contracts/Settings/SettingsDtos.cs`
- Create: `backend/Application/Settings/AlertsSettingsStore.cs`
- Modify: `backend/Api/Controllers/SettingsController.cs`
- Modify: `backend/Api/Program.cs`
- Create: `backend/Api.Tests/SettingsControllerTests.cs`

- [ ] **Step 1: Write the failing settings API tests**

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Settings;
using System.Net;
using System.Net.Http.Json;

namespace OpsMonitor.Api.Tests;

public class SettingsControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Alerts_get_returns_outbound_rule_fields()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        var response = await client.GetAsync("/api/settings/alerts?warehouseId=DE");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AlertsConfigResponse>>();

        Assert.NotNull(payload?.Data);
        Assert.Equal("DE", payload!.Data!.WarehouseId);
        Assert.Equal("Europe/Berlin", payload.Data.TimeZoneId);
        Assert.Equal("16:00", payload.Data.OutboundRule.CutoffTimeStandard);
        Assert.Equal("15:00", payload.Data.OutboundRule.CutoffTimeDaylight);
        Assert.Equal("18:00", payload.Data.OutboundRule.OverdueTime);
        Assert.Equal(1, payload.Data.LeadTimes.Single(x => x.MonitorType == "outbound").LeadTimeHours);
        Assert.Contains("Saturday", payload.Data.WeekendDays);
        Assert.NotEmpty(payload.Data.HolidayDates);
    }

    [Fact]
    public async Task Alerts_put_then_get_persists_outbound_rule_fields_for_warehouse()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        var request = new SaveAlertsConfigRequest(
            WarehouseId: "DE",
            TimeZoneId: "Europe/Berlin",
            WeekendDays: ["Saturday", "Sunday"],
            HolidayDates: ["2026-12-24"],
            LeadTimes:
            [
                new("outbound", 2),
                new("inbound", 12),
                new("shelving", 12),
            ],
            OutboundRule: new("17:00", "16:00", "19:00"),
            SeverityThresholds: [new("outbound", "overdue-count", 25)],
            Receivers: new(["u010"], ["g010"], ["de-ops@example.com"]),
            Channels: [new("wechat", true), new("email", false)]);

        var saveResponse = await client.PutAsJsonAsync("/api/settings/alerts", request);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/settings/alerts?warehouseId=DE");
        var payload = await getResponse.Content.ReadFromJsonAsync<ApiResponse<AlertsConfigResponse>>();

        Assert.NotNull(payload?.Data);
        Assert.Equal("17:00", payload!.Data!.OutboundRule.CutoffTimeStandard);
        Assert.Equal("19:00", payload.Data.OutboundRule.OverdueTime);
        Assert.Equal(2, payload.Data.LeadTimes.Single(x => x.MonitorType == "outbound").LeadTimeHours);
        Assert.Equal(["2026-12-24"], payload.Data.HolidayDates);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~SettingsControllerTests"`

Expected: FAIL because `AlertsConfigResponse` / `SaveAlertsConfigRequest` do not expose the new fields, and the controller does not support warehouse-keyed storage.

- [ ] **Step 3: Add the minimal settings contract fields**

```csharp
namespace OpsMonitor.Contracts.Settings;

public sealed record LeadTimeConfig(string MonitorType, int LeadTimeHours);

public sealed record SeverityThreshold(string MonitorType, string MetricCode, int ThresholdValue);

public sealed record AlertReceivers(
    IReadOnlyList<string> UserIds,
    IReadOnlyList<string> GroupIds,
    IReadOnlyList<string> Emails);

public sealed record AlertChannelConfig(string ChannelCode, bool Enabled);

public sealed record OutboundRuleConfig(
    string CutoffTimeStandard,
    string CutoffTimeDaylight,
    string OverdueTime);

public sealed record AlertsConfigResponse(
    string WarehouseId,
    string TimeZoneId,
    IReadOnlyList<string> WeekendDays,
    IReadOnlyList<string> HolidayDates,
    IReadOnlyList<LeadTimeConfig> LeadTimes,
    OutboundRuleConfig OutboundRule,
    IReadOnlyList<SeverityThreshold> SeverityThresholds,
    AlertReceivers Receivers,
    IReadOnlyList<AlertChannelConfig> Channels,
    DateTimeOffset UpdatedAt,
    string UpdatedBy);

public sealed record SaveAlertsConfigRequest(
    string WarehouseId,
    string TimeZoneId,
    IReadOnlyList<string> WeekendDays,
    IReadOnlyList<string> HolidayDates,
    IReadOnlyList<LeadTimeConfig> LeadTimes,
    OutboundRuleConfig OutboundRule,
    IReadOnlyList<SeverityThreshold> SeverityThresholds,
    AlertReceivers Receivers,
    IReadOnlyList<AlertChannelConfig> Channels);
```

- [ ] **Step 4: Add a shared in-memory settings store keyed by warehouse**

```csharp
using OpsMonitor.Contracts.Settings;

namespace OpsMonitor.Application.Settings;

public sealed class AlertsSettingsStore
{
    private readonly Dictionary<string, AlertsConfigResponse> _configs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DE"] = new(
            WarehouseId: "DE",
            TimeZoneId: "Europe/Berlin",
            WeekendDays: ["Saturday", "Sunday"],
            HolidayDates: ["2026-01-01", "2026-04-03", "2026-04-06"],
            LeadTimes:
            [
                new("outbound", 1),
                new("inbound", 12),
                new("shelving", 12),
            ],
            OutboundRule: new("16:00", "15:00", "18:00"),
            SeverityThresholds: [new("outbound", "overdue-count", 20)],
            Receivers: new(["u001", "u002"], ["g001"], ["ops@example.com"]),
            Channels: [new("wechat", true), new("email", true)],
            UpdatedAt: DateTimeOffset.Now.AddDays(-1),
            UpdatedBy: "admin"),
        ["ON"] = new(
            WarehouseId: "ON",
            TimeZoneId: "America/Toronto",
            WeekendDays: ["Saturday", "Sunday"],
            HolidayDates: ["2026-01-01", "2026-07-01", "2026-12-25"],
            LeadTimes:
            [
                new("outbound", 2),
                new("inbound", 12),
                new("shelving", 12),
            ],
            OutboundRule: new("17:00", "16:00", "19:00"),
            SeverityThresholds: [new("outbound", "overdue-count", 20)],
            Receivers: new(["u101"], ["g101"], ["on@example.com"]),
            Channels: [new("wechat", true), new("email", true)],
            UpdatedAt: DateTimeOffset.Now.AddDays(-1),
            UpdatedBy: "admin"),
    };

    public AlertsConfigResponse Get(string warehouseId)
        => _configs.TryGetValue(warehouseId, out var config) ? config : _configs["DE"];

    public AlertsConfigResponse Save(SaveAlertsConfigRequest request)
    {
        var saved = new AlertsConfigResponse(
            WarehouseId: request.WarehouseId,
            TimeZoneId: request.TimeZoneId,
            WeekendDays: request.WeekendDays,
            HolidayDates: request.HolidayDates,
            LeadTimes: request.LeadTimes,
            OutboundRule: request.OutboundRule,
            SeverityThresholds: request.SeverityThresholds,
            Receivers: request.Receivers,
            Channels: request.Channels,
            UpdatedAt: DateTimeOffset.Now,
            UpdatedBy: "admin");

        _configs[request.WarehouseId] = saved;
        return saved;
    }
}
```

- [ ] **Step 5: Wire the controller and DI to the shared store**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Application.Settings;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Settings;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController(AlertsSettingsStore settingsStore) : ControllerBase
{
    [HttpGet("alerts")]
    public IActionResult GetAlerts([FromQuery] string warehouseId = "DE") =>
        Ok(ApiResponse<AlertsConfigResponse>.Ok(settingsStore.Get(warehouseId)));

    [HttpPut("alerts")]
    public IActionResult SaveAlerts([FromBody] SaveAlertsConfigRequest request) =>
        Ok(ApiResponse<AlertsConfigResponse>.Ok(settingsStore.Save(request)));
}
```

```csharp
using OpsMonitor.Application.Outbound;
using OpsMonitor.Application.Settings;
using OpsMonitor.Infrastructure;
using OpsMonitor.Api.Middleware;
using OpsMonitor.Api.Auth;
using OpsMonitor.Api.BackgroundServices;
using OpsMonitor.Application.Notifications;
using OpsMonitor.Infrastructure.Notifications;
using OpsMonitor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<AlertsSettingsStore>();
builder.Services.AddScoped<WarehouseOutboundRuleProvider>();
builder.Services.AddScoped<WarehouseBusinessCalendarService>();
builder.Services.AddScoped<OutboundDeadlineCalculator>();
```

- [ ] **Step 6: Run test to verify it passes**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~SettingsControllerTests"`

Expected: PASS

- [ ] **Step 7: Commit**

Run:

```bash
git add backend/Contracts/Settings/SettingsDtos.cs backend/Application/Settings/AlertsSettingsStore.cs backend/Api/Controllers/SettingsController.cs backend/Api/Program.cs backend/Api.Tests/SettingsControllerTests.cs
git commit -m "feat(settings): move alerts config into warehouse-backed store"
```

### Task 2: Add A Settings-Backed Outbound Rule Provider And Deadline Calculator

**Files:**
- Create: `backend/Application/Outbound/WarehouseOutboundRuleConfig.cs`
- Create: `backend/Application/Outbound/WarehouseOutboundRuleProvider.cs`
- Create: `backend/Application/Outbound/WarehouseBusinessCalendarService.cs`
- Create: `backend/Application/Outbound/OutboundDeadlineCalculator.cs`
- Create: `backend/Api.Tests/OutboundDeadlineCalculatorTests.cs`

- [ ] **Step 1: Write the failing provider and calculator tests**

```csharp
using OpsMonitor.Application.Outbound;
using OpsMonitor.Application.Settings;
using OpsMonitor.Contracts.Settings;

namespace OpsMonitor.Api.Tests;

public class OutboundDeadlineCalculatorTests
{
    [Fact]
    public void Provider_uses_outbound_lead_time_hours_as_warning_window()
    {
        var store = new AlertsSettingsStore();
        store.Save(new SaveAlertsConfigRequest(
            WarehouseId: "DE",
            TimeZoneId: "Europe/Berlin",
            WeekendDays: ["Saturday", "Sunday"],
            HolidayDates: ["2026-01-01"],
            LeadTimes:
            [
                new("outbound", 2),
                new("inbound", 12),
                new("shelving", 12),
            ],
            OutboundRule: new("16:00", "15:00", "18:00"),
            SeverityThresholds: [new("outbound", "overdue-count", 20)],
            Receivers: new(["u001"], ["g001"], ["ops@example.com"]),
            Channels: [new("wechat", true), new("email", true)]));

        var provider = new WarehouseOutboundRuleProvider(store);

        var rule = provider.GetRule("DE");

        Assert.Equal(TimeSpan.FromHours(2), rule.WarningLeadTime);
    }

    [Fact]
    public void Order_at_summer_cutoff_uses_same_day_deadline()
    {
        var calculator = new OutboundDeadlineCalculator(new WarehouseBusinessCalendarService());
        var rule = CreateGermanRule(TimeSpan.FromHours(1));
        var orderCreatedAtUtc = new DateTimeOffset(2026, 6, 2, 13, 0, 0, TimeSpan.Zero);

        var result = calculator.Calculate(orderCreatedAtUtc, rule);

        Assert.Equal(new DateTimeOffset(2026, 6, 2, 15, 0, 0, TimeSpan.Zero), result.WarningAtUtc);
        Assert.Equal(new DateTimeOffset(2026, 6, 2, 16, 0, 0, TimeSpan.Zero), result.OverdueAtUtc);
    }

    [Fact]
    public void Friday_after_cutoff_skips_to_monday()
    {
        var calculator = new OutboundDeadlineCalculator(new WarehouseBusinessCalendarService());
        var rule = CreateGermanRule(TimeSpan.FromHours(1));
        var orderCreatedAtUtc = new DateTimeOffset(2026, 6, 5, 14, 30, 0, TimeSpan.Zero);

        var result = calculator.Calculate(orderCreatedAtUtc, rule);

        Assert.Equal(new DateTimeOffset(2026, 6, 8, 15, 0, 0, TimeSpan.Zero), result.WarningAtUtc);
        Assert.Equal(new DateTimeOffset(2026, 6, 8, 16, 0, 0, TimeSpan.Zero), result.OverdueAtUtc);
    }

    [Fact]
    public void Holiday_order_skips_to_next_workday()
    {
        var calculator = new OutboundDeadlineCalculator(new WarehouseBusinessCalendarService());
        var rule = CreateGermanRule(TimeSpan.FromHours(1));
        var orderCreatedAtUtc = new DateTimeOffset(2026, 4, 3, 10, 0, 0, TimeSpan.Zero);

        var result = calculator.Calculate(orderCreatedAtUtc, rule);

        Assert.Equal(new DateTimeOffset(2026, 4, 7, 15, 0, 0, TimeSpan.Zero), result.WarningAtUtc);
        Assert.Equal(new DateTimeOffset(2026, 4, 7, 16, 0, 0, TimeSpan.Zero), result.OverdueAtUtc);
    }

    [Fact]
    public void Winter_workday_uses_standard_cutoff()
    {
        var calculator = new OutboundDeadlineCalculator(new WarehouseBusinessCalendarService());
        var rule = CreateGermanRule(TimeSpan.FromHours(1));
        var orderCreatedAtUtc = new DateTimeOffset(2026, 1, 6, 14, 30, 0, TimeSpan.Zero);

        var result = calculator.Calculate(orderCreatedAtUtc, rule);

        Assert.Equal(new DateTimeOffset(2026, 1, 6, 16, 0, 0, TimeSpan.Zero), result.WarningAtUtc);
        Assert.Equal(new DateTimeOffset(2026, 1, 6, 17, 0, 0, TimeSpan.Zero), result.OverdueAtUtc);
    }

    private static WarehouseOutboundRuleConfig CreateGermanRule(TimeSpan warningLeadTime) => new(
        TimeZoneId: "Europe/Berlin",
        WeekendDays: ["Saturday", "Sunday"],
        HolidayDates: [new DateOnly(2026, 1, 1), new DateOnly(2026, 4, 3), new DateOnly(2026, 4, 6)],
        CutoffTimeStandard: new TimeOnly(16, 0),
        CutoffTimeDaylight: new TimeOnly(15, 0),
        OverdueTime: new TimeOnly(18, 0),
        WarningLeadTime: warningLeadTime);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~OutboundDeadlineCalculatorTests"`

Expected: FAIL because the outbound rule provider, config type, and calculator services do not exist yet.

- [ ] **Step 3: Add the outbound rule config and settings-backed provider**

```csharp
namespace OpsMonitor.Application.Outbound;

public sealed record WarehouseOutboundRuleConfig(
    string TimeZoneId,
    IReadOnlyList<string> WeekendDays,
    IReadOnlyList<DateOnly> HolidayDates,
    TimeOnly CutoffTimeStandard,
    TimeOnly CutoffTimeDaylight,
    TimeOnly OverdueTime,
    TimeSpan WarningLeadTime);

public sealed record OutboundDeadlineResult(
    DateTimeOffset WarningAtUtc,
    DateTimeOffset OverdueAtUtc,
    string TimeZoneId);
```

```csharp
using OpsMonitor.Application.Settings;

namespace OpsMonitor.Application.Outbound;

public sealed class WarehouseOutboundRuleProvider(AlertsSettingsStore settingsStore)
{
    public WarehouseOutboundRuleConfig GetRule(string warehouseId)
    {
        var config = settingsStore.Get(warehouseId);
        var outboundLeadTime = config.LeadTimes.Single(x => x.MonitorType == "outbound");

        return new WarehouseOutboundRuleConfig(
            TimeZoneId: config.TimeZoneId,
            WeekendDays: config.WeekendDays,
            HolidayDates: config.HolidayDates.Select(DateOnly.Parse).ToArray(),
            CutoffTimeStandard: TimeOnly.Parse(config.OutboundRule.CutoffTimeStandard),
            CutoffTimeDaylight: TimeOnly.Parse(config.OutboundRule.CutoffTimeDaylight),
            OverdueTime: TimeOnly.Parse(config.OutboundRule.OverdueTime),
            WarningLeadTime: TimeSpan.FromHours(outboundLeadTime.LeadTimeHours));
    }
}
```

- [ ] **Step 4: Add the business-calendar service and deadline calculator**

```csharp
namespace OpsMonitor.Application.Outbound;

public sealed class WarehouseBusinessCalendarService
{
    public bool IsWorkingDay(DateOnly date, WarehouseOutboundRuleConfig rule)
    {
        var day = date.DayOfWeek.ToString();
        return !rule.WeekendDays.Contains(day) && !rule.HolidayDates.Contains(date);
    }

    public DateOnly GetNextWorkingDay(DateOnly date, WarehouseOutboundRuleConfig rule)
    {
        var current = date;
        do
        {
            current = current.AddDays(1);
        } while (!IsWorkingDay(current, rule));

        return current;
    }
}
```

```csharp
namespace OpsMonitor.Application.Outbound;

public sealed class OutboundDeadlineCalculator(WarehouseBusinessCalendarService calendar)
{
    public OutboundDeadlineResult Calculate(DateTimeOffset orderCreatedAtUtc, WarehouseOutboundRuleConfig rule)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(rule.TimeZoneId);
        var localOrder = TimeZoneInfo.ConvertTime(orderCreatedAtUtc, timeZone);
        var localDate = DateOnly.FromDateTime(localOrder.DateTime);
        var localTime = TimeOnly.FromDateTime(localOrder.DateTime);

        DateOnly promiseDate;
        if (!calendar.IsWorkingDay(localDate, rule))
        {
            promiseDate = calendar.GetNextWorkingDay(localDate, rule);
        }
        else
        {
            var cutoff = timeZone.IsDaylightSavingTime(localOrder.DateTime)
                ? rule.CutoffTimeDaylight
                : rule.CutoffTimeStandard;

            promiseDate = localTime <= cutoff
                ? localDate
                : calendar.GetNextWorkingDay(localDate, rule);
        }

        var overdueLocal = promiseDate.ToDateTime(rule.OverdueTime, DateTimeKind.Unspecified);
        var overdueUtc = new DateTimeOffset(overdueLocal, timeZone.GetUtcOffset(overdueLocal)).ToUniversalTime();
        var warningUtc = overdueUtc - rule.WarningLeadTime;

        return new OutboundDeadlineResult(warningUtc, overdueUtc, rule.TimeZoneId);
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~OutboundDeadlineCalculatorTests"`

Expected: PASS

- [ ] **Step 6: Commit**

Run:

```bash
git add backend/Application/Outbound/WarehouseOutboundRuleConfig.cs backend/Application/Outbound/WarehouseOutboundRuleProvider.cs backend/Application/Outbound/WarehouseBusinessCalendarService.cs backend/Application/Outbound/OutboundDeadlineCalculator.cs backend/Api.Tests/OutboundDeadlineCalculatorTests.cs
git commit -m "feat(backend): add warehouse outbound rule provider and calculator"
```

### Task 3: Use The Shared Provider And Calculator In Outbound APIs

**Files:**
- Modify: `backend/Api/Controllers/AnomaliesController.cs`
- Modify: `backend/Api/Controllers/DashboardController.cs`
- Modify: `backend/Api.Tests/AnomaliesControllerTests.cs`
- Modify: `backend/Api.Tests/DashboardControllerTests.cs`

- [ ] **Step 1: Strengthen the outbound API tests before refactoring the controllers**

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Anomalies;
using System.Net;
using System.Net.Http.Json;

namespace OpsMonitor.Api.Tests;

public class AnomaliesControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Outbound_with_riskStatus_overdue_returns_only_overdue_unshipped_items()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        var response = await client.GetAsync("/api/anomalies/outbound?warehouseCode=DE&riskStatus=overdue");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<OutboundResponse>>();
        Assert.NotNull(payload?.Data);
        Assert.NotEmpty(payload!.Data!.List.Items);
        Assert.All(payload.Data.List.Items, item => Assert.Equal("overdue", item.RiskStatus));
        Assert.All(payload.Data.List.Items, item => Assert.False(item.Shipped));
        Assert.All(payload.Data.List.Items, item => Assert.NotEqual(default, item.DeadlineAt));
    }
}
```

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Dashboard;
using System.Net;
using System.Net.Http.Json;

namespace OpsMonitor.Api.Tests;

public class DashboardControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Dashboard_outbound_preview_counts_match_item_risk_status()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        var response = await client.GetAsync("/api/dashboard?warehouseCode=DE");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardResponse>>();
        var outbound = payload!.Data!.AnomalyPreview.Outbound;

        Assert.Equal(outbound.Items.Count, outbound.Total);
        Assert.Equal(outbound.Items.Count(x => x.RiskStatus == "imminent"), outbound.ImminentCount);
        Assert.Equal(outbound.Items.Count(x => x.RiskStatus == "overdue"), outbound.OverdueCount);
        Assert.All(outbound.Items, item => Assert.NotEqual(default, item.DeadlineAt));
    }
}
```

- [ ] **Step 2: Run tests to verify they fail against the current hard-coded controller logic**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~AnomaliesControllerTests|FullyQualifiedName~DashboardControllerTests"`

Expected: FAIL because the outbound anomaly endpoint ignores `riskStatus`, and the dashboard preview counts are hard-coded instead of derived from the returned items.

- [ ] **Step 3: Inject the rule provider and calculator into the outbound controller flow**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Application.Outbound;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Anomalies;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/anomalies")]
[Authorize]
public class AnomaliesController(
    WarehouseOutboundRuleProvider ruleProvider,
    OutboundDeadlineCalculator deadlineCalculator) : ControllerBase
{
    [HttpGet("outbound")]
    public IActionResult GetOutbound(
        [FromQuery] string warehouseCode = "DE",
        [FromQuery] string? riskStatus = null,
        [FromQuery] string? channel = null,
        [FromQuery] string? customer = null,
        [FromQuery] DateTimeOffset? orderTimeStart = null,
        [FromQuery] DateTimeOffset? orderTimeEnd = null,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "deadlineAt",
        [FromQuery] string sortDirection = "asc")
    {
        var now = DateTimeOffset.Now;
        var orderPrefix = warehouseCode == "ON" ? "ON" : "DE";
        var rule = ruleProvider.GetRule(warehouseCode);

        var seeds = new[]
        {
            new { OrderId = $"{orderPrefix}-SO20260504001", CustomerOrChannel = "客户A / 渠道A", OrderTime = now.AddHours(-30), CurrentStatus = "待出库", Shipped = false },
            new { OrderId = $"{orderPrefix}-SO20260504002", CustomerOrChannel = "客户B / 渠道B", OrderTime = now.AddHours(-4), CurrentStatus = "待出库", Shipped = false },
            new { OrderId = $"{orderPrefix}-SO20260504003", CustomerOrChannel = "客户C / 渠道C", OrderTime = now.AddHours(-2), CurrentStatus = "已出库", Shipped = true },
        };

        var items = seeds.Select(seed =>
        {
            var deadline = deadlineCalculator.Calculate(seed.OrderTime, rule);
            var warningReached = now >= deadline.WarningAtUtc;
            var overdueReached = now >= deadline.OverdueAtUtc;
            var remainingMinutes = (int)Math.Round((deadline.OverdueAtUtc - now).TotalMinutes);
            var overdueMinutes = (int)Math.Round((now - deadline.OverdueAtUtc).TotalMinutes);
            var computedRiskStatus = overdueReached ? "overdue" : warningReached ? "imminent" : "normal";

            return new OutboundItem(
                OrderId: seed.OrderId,
                CustomerOrChannel: seed.CustomerOrChannel,
                OrderTime: seed.OrderTime,
                DeadlineAt: deadline.OverdueAtUtc,
                TimeStatus: overdueReached ? "overdue" : "remaining",
                TimeValueMinutes: overdueReached ? Math.Abs(overdueMinutes) : remainingMinutes,
                TimeValueLabel: overdueReached
                    ? $"超时 {Math.Abs(overdueMinutes) / 60.0:F1} 小时"
                    : $"剩余 {remainingMinutes / 60.0:F1} 小时",
                CurrentStatus: seed.CurrentStatus,
                Shipped: seed.Shipped,
                RiskStatus: computedRiskStatus);
        })
        .Where(x => !x.Shipped)
        .Where(x => x.RiskStatus is "imminent" or "overdue")
        .ToList();

        if (!string.IsNullOrWhiteSpace(riskStatus))
        {
            items = items.Where(x => x.RiskStatus == riskStatus).ToList();
        }

        var imminentCount = items.Count(x => x.RiskStatus == "imminent");
        var overdueCount = items.Count(x => x.RiskStatus == "overdue");

        var data = new OutboundResponse(
            Summary: new(items.Count, imminentCount, overdueCount),
            FilterOptions: new(["渠道A", "渠道B", "渠道C"], ["客户A", "客户B", "客户C"]),
            List: new(pageNo, pageSize, items.Count, items));

        return Ok(ApiResponse<OutboundResponse>.Ok(data));
    }
}
```

- [ ] **Step 4: Derive dashboard outbound preview counts from the same calculated rule output**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Application.Outbound;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Dashboard;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(
    WarehouseOutboundRuleProvider ruleProvider,
    OutboundDeadlineCalculator deadlineCalculator) : ControllerBase
{
    private static readonly TimeSpan DelayThreshold = TimeSpan.FromMinutes(90);

    [HttpGet]
    public IActionResult Get([FromQuery] string warehouseCode = "DE")
    {
        var now = DateTimeOffset.Now;
        var lastSync = now.AddMinutes(-30);
        var delayedDataFlag = (now - lastSync) > DelayThreshold;

        var warehouse = warehouseCode == "ON"
            ? new WarehouseInfo("ON", "Ontario")
            : new WarehouseInfo("DE", "Germany");

        var orderPrefix = warehouseCode == "ON" ? "ON" : "DE";
        var rule = ruleProvider.GetRule(warehouseCode);

        var outboundItems = new[]
        {
            new { OrderId = $"{orderPrefix}-SO20260504001", CustomerOrChannel = "CustomerA / ChannelA", OrderTime = now.AddHours(-30), CurrentStatus = "pending-outbound", Shipped = false },
            new { OrderId = $"{orderPrefix}-SO20260504002", CustomerOrChannel = "CustomerB / ChannelB", OrderTime = now.AddHours(-4), CurrentStatus = "pending-outbound", Shipped = false },
            new { OrderId = $"{orderPrefix}-SO20260504003", CustomerOrChannel = "CustomerC / ChannelC", OrderTime = now.AddHours(-2), CurrentStatus = "shipped", Shipped = true },
        }
        .Select(seed =>
        {
            var deadline = deadlineCalculator.Calculate(seed.OrderTime, rule);
            var warningReached = now >= deadline.WarningAtUtc;
            var overdueReached = now >= deadline.OverdueAtUtc;
            var remainingMinutes = (int)Math.Round((deadline.OverdueAtUtc - now).TotalMinutes);
            var overdueMinutes = (int)Math.Round((now - deadline.OverdueAtUtc).TotalMinutes);
            var riskStatus = overdueReached ? "overdue" : warningReached ? "imminent" : "normal";

            return new
            {
                seed.OrderId,
                seed.CustomerOrChannel,
                seed.OrderTime,
                seed.CurrentStatus,
                seed.Shipped,
                DeadlineAt = deadline.OverdueAtUtc,
                TimeStatus = overdueReached ? "overdue" : "remaining",
                TimeValueMinutes = overdueReached ? Math.Abs(overdueMinutes) : remainingMinutes,
                TimeValueLabel = overdueReached
                    ? $"超时 {Math.Abs(overdueMinutes) / 60.0:F1} 小时"
                    : $"剩余 {remainingMinutes / 60.0:F1} 小时",
                RiskStatus = riskStatus,
            };
        })
        .Where(x => !x.Shipped)
        .Where(x => x.RiskStatus is "imminent" or "overdue")
        .Select(x => new AnomalyPreviewItem(
            OrderId: x.OrderId,
            AsnId: null,
            CartonId: null,
            CustomerOrChannel: x.CustomerOrChannel,
            OrderTime: x.OrderTime,
            FirstArrivalTime: null,
            ArrivalTime: null,
            DeadlineAt: x.DeadlineAt,
            CurrentStatus: x.CurrentStatus,
            RiskStatus: x.RiskStatus,
            TimeStatus: x.TimeStatus,
            TimeValueMinutes: x.TimeValueMinutes,
            TimeValueLabel: x.TimeValueLabel,
            PlannedCartonCount: null,
            ArrivedCartonCount: null,
            MissingCartonCount: null,
            SkuCount: null,
            UnshelvedSkuCount: null))
        .ToList();

        var outboundPreview = new AnomalyPreviewGroup(
            Total: outboundItems.Count,
            ImminentCount: outboundItems.Count(x => x.RiskStatus == "imminent"),
            OverdueCount: outboundItems.Count(x => x.RiskStatus == "overdue"),
            Items: outboundItems);

        var inboundItems = Enumerable.Range(1, 5).Select(i =>
        {
            var isOverdue = i % 2 == 0;
            var minutesDiff = isOverdue ? -(i * 60) : (i * 120);
            return new AnomalyPreviewItem(
                OrderId: null,
                AsnId: $"ASN2026050400{i}",
                CartonId: null,
                CustomerOrChannel: null,
                OrderTime: null,
                FirstArrivalTime: now.AddDays(-i),
                ArrivalTime: null,
                DeadlineAt: now.AddMinutes(minutesDiff),
                CurrentStatus: null,
                RiskStatus: isOverdue ? "overdue" : "imminent",
                TimeStatus: isOverdue ? "overdue" : "remaining",
                TimeValueMinutes: Math.Abs(minutesDiff),
                TimeValueLabel: isOverdue
                    ? $"超时 {Math.Abs(minutesDiff) / 60.0:F1} 小时"
                    : $"剩余 {minutesDiff / 60.0:F1} 小时",
                PlannedCartonCount: 50,
                ArrivedCartonCount: 50 - i * 2,
                MissingCartonCount: i * 2,
                SkuCount: null,
                UnshelvedSkuCount: null);
        }).ToList();

        var shelvingItems = Enumerable.Range(1, 5).Select(i =>
        {
            var isOverdue = i % 3 == 0;
            var minutesDiff = isOverdue ? -(i * 30) : (i * 90);
            return new AnomalyPreviewItem(
                OrderId: null,
                AsnId: $"ASN202605030{i:D2}",
                CartonId: $"CTN2026050400{i}",
                CustomerOrChannel: null,
                OrderTime: null,
                FirstArrivalTime: null,
                ArrivalTime: now.AddDays(-i),
                DeadlineAt: now.AddMinutes(minutesDiff),
                CurrentStatus: null,
                RiskStatus: isOverdue ? "overdue" : "imminent",
                TimeStatus: isOverdue ? "overdue" : "remaining",
                TimeValueMinutes: Math.Abs(minutesDiff),
                TimeValueLabel: isOverdue
                    ? $"超时 {Math.Abs(minutesDiff) / 60.0:F1} 小时"
                    : $"剩余 {minutesDiff / 60.0:F1} 小时",
                PlannedCartonCount: null,
                ArrivedCartonCount: null,
                MissingCartonCount: null,
                SkuCount: 18,
                UnshelvedSkuCount: i * 2);
        }).ToList();

        var forecast = Enumerable.Range(1, 7).Select(i => new
        {
            Date = now.AddDays(i).ToString("yyyy-MM-dd"),
            CartonCount = 100 + i * 8,
            Weight = 1200 + i * 50.0,
            Volume = 28 + i * 0.8,
        }).ToList();

        var avgCartons = forecast.Average(f => f.CartonCount);
        var forecast7Days = forecast.Select(f => new Forecast7DaysItem(
            Date: f.Date,
            CartonCount: f.CartonCount,
            Weight: f.Weight,
            Volume: f.Volume,
            IsPeakDay: f.CartonCount > avgCartons * 1.3)).ToList();

        var data = new DashboardResponse(
            Warehouse: warehouse,
            BaseStatus: new(
                LastSyncTime: lastSync,
                SyncStatus: delayedDataFlag ? "delayed" : "success",
                TodayReminderCount: 16,
                TodayOverdueCount: outboundPreview.OverdueCount + inboundItems.Count(x => x.RiskStatus == "overdue") + shelvingItems.Count(x => x.RiskStatus == "overdue"),
                AlertChannels:
                [
                    new("wechat", "WeCom", "healthy"),
                    new("email", "Email", "healthy"),
                ],
                DelayedDataFlag: delayedDataFlag,
                DelayedReason: delayedDataFlag ? "Business system sync delayed" : null),
            TodayOverview: new(
                OutboundRiskCount: outboundPreview.Total,
                InboundRiskCount: inboundItems.Count,
                ShelvingRiskCount: shelvingItems.Count,
                TodayVolumePressureLevel: "medium",
                OperationTip: "Prioritize outbound tasks first."),
            AnomalyPreview: new(
                Outbound: outboundPreview,
                Inbound: new(
                    Total: inboundItems.Count,
                    ImminentCount: inboundItems.Count(x => x.RiskStatus == "imminent"),
                    OverdueCount: inboundItems.Count(x => x.RiskStatus == "overdue"),
                    Items: inboundItems),
                Shelving: new(
                    Total: shelvingItems.Count,
                    ImminentCount: shelvingItems.Count(x => x.RiskStatus == "imminent"),
                    OverdueCount: shelvingItems.Count(x => x.RiskStatus == "overdue"),
                    Items: shelvingItems)),
            Forecast7Days: forecast7Days);

        return Ok(ApiResponse<DashboardResponse>.Ok(data));
    }
}
```

- [ ] **Step 5: Run tests to verify the endpoint behavior passes with the shared rule source**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~AnomaliesControllerTests|FullyQualifiedName~DashboardControllerTests"`

Expected: PASS

- [ ] **Step 6: Commit**

Run:

```bash
git add backend/Api/Controllers/AnomaliesController.cs backend/Api/Controllers/DashboardController.cs backend/Api.Tests/AnomaliesControllerTests.cs backend/Api.Tests/DashboardControllerTests.cs
git commit -m "feat(api): drive outbound anomaly timing from saved warehouse rules"
```

### Task 4: Expose The New Settings Fields And Outbound Warning Lead In The Frontend

**Files:**
- Modify: `frontend/src/services/types.ts`
- Modify: `frontend/src/modules/settings/views/AlertsConfigView.vue`
- Optional: `frontend/src/services/api.ts`

- [ ] **Step 1: Add frontend type coverage for the new settings fields**

```ts
export interface OutboundRuleConfig {
  cutoffTimeStandard: string
  cutoffTimeDaylight: string
  overdueTime: string
}

export interface AlertsConfigData {
  warehouseId: string
  timeZoneId: string
  weekendDays: string[]
  holidayDates: string[]
  leadTimes: LeadTimeConfig[]
  outboundRule: OutboundRuleConfig
  severityThresholds: SeverityThreshold[]
  receivers: AlertReceivers
  channels: AlertChannelConfig[]
  updatedAt: string
  updatedBy: string
}
```

- [ ] **Step 2: Run frontend build to capture a clean baseline before the page change**

Run: `npm.cmd --prefix frontend run build`

Expected: PASS

- [ ] **Step 3: Move the outbound lead time into the outbound rule block and keep inbound/shelving in the existing lead-time section**

```vue
<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { settingsApi } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'
import type { AlertsConfigData } from '@/services/types'

const authStore = useAuthStore()
const loading = ref(false)
const saving = ref(false)
const config = ref<AlertsConfigData | null>(null)

const outboundLeadTime = computed(
  () => config.value?.leadTimes.find(item => item.monitorType === 'outbound') ?? null,
)

const otherLeadTimes = computed(
  () => config.value?.leadTimes.filter(item => item.monitorType !== 'outbound') ?? [],
)
</script>
```

```vue
<div class="section-card">
  <div class="section-title">出库规则配置</div>
  <div class="field-grid">
    <div class="field-row">
      <label>仓库时区</label>
      <el-input v-model="config.timeZoneId" />
    </div>
    <div class="field-row">
      <label>冬令时截单时间</label>
      <el-input v-model="config.outboundRule.cutoffTimeStandard" />
    </div>
    <div class="field-row">
      <label>夏令时截单时间</label>
      <el-input v-model="config.outboundRule.cutoffTimeDaylight" />
    </div>
    <div class="field-row">
      <label>超时时点</label>
      <el-input v-model="config.outboundRule.overdueTime" />
    </div>
    <div v-if="outboundLeadTime" class="field-row">
      <label>出库预警提前量（小时）</label>
      <el-input-number v-model="outboundLeadTime.leadTimeHours" :min="1" :max="72" size="small" />
    </div>
  </div>
</div>

<div class="section-card">
  <div class="section-title">工作日历配置</div>
  <div class="field-grid">
    <div class="field-row">
      <label>周末定义</label>
      <el-select v-model="config.weekendDays" multiple>
        <el-option label="Saturday" value="Saturday" />
        <el-option label="Sunday" value="Sunday" />
      </el-select>
    </div>
    <div class="field-row holiday-block">
      <label>法定节假日</label>
      <div class="holiday-tags">
        <el-tag
          v-for="(holiday, index) in config.holidayDates"
          :key="holiday"
          closable
          @close="config!.holidayDates.splice(index, 1)"
        >{{ holiday }}</el-tag>
      </div>
    </div>
  </div>
</div>

<div class="section-card">
  <div class="section-title">提前预警时长（小时）</div>
  <div class="field-grid">
    <div v-for="lt in otherLeadTimes" :key="lt.monitorType" class="field-row">
      <label>{{ monitorTypeLabel(lt.monitorType) }}</label>
      <el-input-number v-model="lt.leadTimeHours" :min="1" :max="72" size="small" />
    </div>
  </div>
</div>
```

- [ ] **Step 4: Keep the save payload shape unchanged apart from the new settings fields**

```ts
async function save() {
  if (!config.value) return
  saving.value = true
  try {
    const { updatedAt, updatedBy, ...payload } = config.value
    const res = await settingsApi.saveAlerts(payload)
    config.value = res.data.data ?? config.value
    ElMessage.success('保存成功')
  } catch {
    ElMessage.error('保存失败，请重试')
  } finally {
    saving.value = false
  }
}
```

- [ ] **Step 5: Run frontend build to verify it passes**

Run: `npm.cmd --prefix frontend run build`

Expected: PASS

- [ ] **Step 6: Commit**

Run:

```bash
git add frontend/src/services/types.ts frontend/src/modules/settings/views/AlertsConfigView.vue
git commit -m "feat(frontend): expose warehouse outbound timing settings"
```

### Task 5: Final Regression Verification

**Files:**
- Verify only

- [ ] **Step 1: Run the backend test slice for settings, calculator, anomalies, and dashboard**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~SettingsControllerTests|FullyQualifiedName~OutboundDeadlineCalculatorTests|FullyQualifiedName~AnomaliesControllerTests|FullyQualifiedName~DashboardControllerTests"`

Expected: PASS

- [ ] **Step 2: Run the frontend production build**

Run: `npm.cmd --prefix frontend run build`

Expected: PASS

- [ ] **Step 3: Review the local diff for scope control**

Run: `git diff --stat HEAD~4..HEAD`

Expected: Only settings contract/store/controller, outbound timing services, outbound API controllers/tests, and settings frontend files changed.

- [ ] **Step 4: Commit any final fixups if verification exposed a small issue**

Run:

```bash
git add backend/Api.Tests backend/Api frontend/src/modules/settings/views/AlertsConfigView.vue frontend/src/services/types.ts
git commit -m "test: verify warehouse outbound timing config flow"
```

## Self-Review

- Spec coverage: The revised plan now covers warehouse-keyed settings storage, shared rule mapping, DST-aware deadline calculation, weekend/holiday skipping, risk-status derivation, outbound anomaly/dashboard integration, and the settings-page fields called for by the spec. It still intentionally excludes multi-warehouse frontend switching, inbound/shelving rule refactors, and holiday import/sync.

- Placeholder scan: The previous undefined `orderTime` reference is removed. The fake “frontend build should fail after only changing types” checkpoint is gone. Each execution task now has a direct file target, concrete code, and a real verification command.

- Type consistency: `leadTimes.outbound` is used consistently as the outbound warning lead in hours across settings DTOs, the rule provider, calculator tests, and the frontend page. `warehouseId` / `warehouseCode` are now connected through the shared settings store and `WarehouseOutboundRuleProvider`.

Plan revised and saved to `docs/superpowers/plans/2026-05-08-warehouse-outbound-timeout-config-implementation-plan.md`. Two execution options:

1. Subagent-Driven (recommended) - I dispatch a fresh subagent per task, review between tasks, fast iteration

2. Inline Execution - Execute tasks in this session using executing-plans, batch execution with checkpoints

Which approach?
