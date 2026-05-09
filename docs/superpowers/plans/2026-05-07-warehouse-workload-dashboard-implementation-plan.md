# Warehouse Workload Dashboard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the new warehouse workload dashboard as the default homepage, keep the current dashboard as the anomaly dashboard, and add the four approved drilldown pages with query and export support.

**Architecture:** Keep the current anomaly dashboard API and view on a parallel route instead of rewriting them in place. Add a dedicated workload API surface, a new frontend workload module, and route/query wiring that carries `warehouseCode`, time ranges, and status filters from dashboard cards into detail pages and anomaly pages.

**Tech Stack:** Vue 3, Vue Router, Pinia, Element Plus, ECharts, Axios, Vitest, ASP.NET Core 10 Web API, C# record DTOs, xUnit integration tests.

---

## Scope Decisions Locked In

- Keep `/api/dashboard` and `frontend/src/modules/dashboard/views/DashboardView.vue` as the anomaly dashboard implementation.
- Add new workload endpoints under `/api/workloads/*` instead of overloading the anomaly dashboard contract.
- Make `/dashboard` the workload dashboard route and move the current dashboard UI to `/anomaly-dashboard`.
- Use `warehouseCode` query parameters everywhere the user can switch warehouses: workload dashboard, anomaly dashboard, anomaly detail pages, and workload detail pages.
- Use client-side CSV export for MVP detail pages so export always matches the currently loaded filtered result set without adding separate backend file endpoints.
- Use these route names and permissions:
  - `dashboard` / `workload-dashboard`
  - `anomaly-dashboard` / `anomaly-dashboard`
  - `workload-outbound-detail` / `workload-outbound-detail`
  - `workload-sku-detail` / `workload-sku-detail`
  - `workload-carton-detail` / `workload-carton-detail`
  - `workload-future-inbound-volume` / `workload-future-inbound-volume`
  - existing anomaly detail routes keep their current names and permissions

## File Structure

### Backend

- Create: `backend/Contracts/Workload/WorkloadDtos.cs`
  - Workload dashboard DTOs
  - Today/tomorrow inbound transport rows
  - Four detail-page DTOs
- Create: `backend/Api/Controllers/WorkloadsController.cs`
  - Authorized mock endpoints for workload dashboard and four detail pages
  - Shared warehouse/date/status/transport filtering helpers
- Create: `backend/Api.Tests/WorkloadControllerTests.cs`
  - Integration tests for workload endpoints and payload shape
- Create: `backend/Api.Tests/DashboardControllerTests.cs`
  - Integration tests for anomaly dashboard warehouse switching
- Create: `backend/Api.Tests/AnomaliesControllerTests.cs`
  - Integration tests for anomaly detail warehouse and overdue query binding
- Modify: `backend/Api/Controllers/AuthController.cs`
  - Return new menu permission codes
- Modify: `backend/Api/Controllers/DashboardController.cs`
  - Accept `warehouseCode`
  - Return the selected warehouse in the anomaly dashboard payload
- Modify: `backend/Api/Controllers/AnomaliesController.cs`
  - Accept `warehouseCode`
  - Keep existing filters working alongside warehouse filtering
- Modify: `backend/Api.Tests/AuthControllerTests.cs`
  - Assert new permission payload

### Frontend

- Create: `frontend/src/modules/workload/views/WorkloadDashboardView.vue`
  - New default homepage
- Create: `frontend/src/modules/workload/views/OutboundPackagesDetailView.vue`
  - Today outbound packages detail page
- Create: `frontend/src/modules/workload/views/SkuDetailView.vue`
  - Today shelving SKU detail page
- Create: `frontend/src/modules/workload/views/CartonDetailView.vue`
  - Today/tomorrow inbound cartons shared detail page
- Create: `frontend/src/modules/workload/views/FutureInboundVolumeDetailView.vue`
  - Future 7-day inbound volume detail page
- Create: `frontend/src/modules/workload/components/FutureInboundForecastChart.vue`
  - ECharts bar chart for the 7-day forecast card
- Create: `frontend/src/modules/workload/components/InboundMetricsMatrix.vue`
  - Shared matrix for today/tomorrow inbound metric blocks
- Create: `frontend/src/modules/workload/utils/datePresets.ts`
  - `today`, `tomorrow`, `future7days` range helpers
- Create: `frontend/src/modules/workload/utils/exportCsv.ts`
  - Client-side CSV download helper
- Create: `frontend/src/modules/workload/utils/formatters.ts`
  - Shared label/date/number formatting helpers
- Create: `frontend/src/modules/workload/__tests__/workload-routes.spec.ts`
  - Route and redirect assertions for workload/anomaly navigation
- Create: `frontend/src/modules/workload/__tests__/exportCsv.spec.ts`
  - CSV helper tests
- Modify: `frontend/src/router/routes.ts`
  - Register the new workload routes and the anomaly dashboard route
- Modify: `frontend/src/router/index.ts`
  - Keep auth redirect pointing to `/dashboard`
- Modify: `frontend/src/layouts/AppShell.vue`
  - Update menu labels and top-level entries
- Modify: `frontend/src/stores/tabs.ts`
  - Default tab becomes `工作量看板`
- Modify: `frontend/src/shared/constants/permissions.ts`
  - Add workload/anomaly dashboard permission constants
- Modify: `frontend/src/services/types.ts`
  - Add workload interfaces and extend anomaly query interfaces with `warehouseCode`
- Modify: `frontend/src/services/api.ts`
  - Add `workloadApi`
  - Add `warehouseCode` pass-through for anomaly and anomaly-dashboard calls
- Modify: `frontend/src/modules/dashboard/views/DashboardView.vue`
  - Becomes the anomaly dashboard entry with warehouse switching
- Modify: `frontend/src/modules/anomalies/views/OutboundView.vue`
  - Read `warehouseCode` and `riskStatus` from route query
- Modify: `frontend/src/modules/anomalies/views/InboundView.vue`
  - Read `warehouseCode` and `riskStatus` from route query
- Modify: `frontend/src/modules/anomalies/views/ShelvingView.vue`
  - Read `warehouseCode` and `riskStatus` from route query

## Interface Contracts

### Frontend Routes

- `/dashboard`
- `/anomaly-dashboard`
- `/workloads/outbound-packages`
- `/workloads/skus`
- `/workloads/cartons`
- `/workloads/future-inbound-volume`
- `/anomalies/outbound`
- `/anomalies/inbound`
- `/anomalies/shelving`

### Backend Endpoints

- `GET /api/dashboard?warehouseCode=DE`
- `GET /api/anomalies/outbound?warehouseCode=DE&riskStatus=overdue`
- `GET /api/anomalies/inbound?warehouseCode=DE&riskStatus=overdue`
- `GET /api/anomalies/shelving?warehouseCode=DE&riskStatus=overdue`
- `GET /api/workloads/dashboard?warehouseCode=DE`
- `GET /api/workloads/outbound-packages?warehouseCode=DE&dateStart=2026-05-07&dateEnd=2026-05-07&processingStatus=all&keyword=`
- `GET /api/workloads/skus?warehouseCode=DE&dateStart=2026-05-07&dateEnd=2026-05-07&processingStatus=all&transportMode=&keyword=`
- `GET /api/workloads/cartons?warehouseCode=DE&dateStart=2026-05-07&dateEnd=2026-05-07&processingStatus=all&transportMode=&keyword=&context=today`
- `GET /api/workloads/future-inbound-volume?warehouseCode=DE&dateStart=2026-05-08&dateEnd=2026-05-16&transportMode=`

### Shared Query Vocabulary

- `warehouseCode`: `DE | ON`
- `processingStatus`: `all | processed | pending`
- `transportMode`: `sea | air | express | truck`
- `context`: `today | tomorrow`

### Open Data Mapping Assumptions

- “Today” and “tomorrow” ranges are routed as date ranges from the frontend. Backend mock data should simply honor the incoming dates without baking business-day logic into controller code.
- Export is CSV generated from the currently loaded filtered rows.
- Mock data must expose separate `seaContainerCount` and `truckPalletCount` fields because both the chart tooltip and the future inbound detail page need them.

### Task 1: Lock Permission Codes And Navigation Vocabulary

**Files:**
- Modify: `backend/Api/Controllers/AuthController.cs`
- Modify: `backend/Api.Tests/AuthControllerTests.cs`
- Modify: `frontend/src/shared/constants/permissions.ts`

- [ ] **Step 1: Write the failing backend permission test**

```csharp
[Fact]
public async Task Auth_permissions_returns_workload_and_anomaly_menu_permissions()
{
    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

    var response = await client.GetAsync("/api/auth/permissions");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PermissionsResponse>>();
    Assert.Contains("workload-dashboard", result!.Data!.MenuPermissions);
    Assert.Contains("anomaly-dashboard", result.Data.MenuPermissions);
    Assert.Contains("workload-outbound-detail", result.Data.MenuPermissions);
    Assert.Contains("workload-sku-detail", result.Data.MenuPermissions);
    Assert.Contains("workload-carton-detail", result.Data.MenuPermissions);
    Assert.Contains("workload-future-inbound-volume", result.Data.MenuPermissions);
}
```

- [ ] **Step 2: Run the auth test and confirm it fails on missing permissions**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~Auth_permissions_returns_workload_and_anomaly_menu_permissions"`

Expected: FAIL because `workload-dashboard` and `anomaly-dashboard` are not in the permission payload yet.

- [ ] **Step 3: Update backend and frontend permission vocabularies**

```csharp
var data = new PermissionsResponse(
    MenuPermissions:
    [
        "workload-dashboard",
        "anomaly-dashboard",
        "workload-outbound-detail",
        "workload-sku-detail",
        "workload-carton-detail",
        "workload-future-inbound-volume",
        "anomaly-outbound",
        "anomaly-inbound",
        "anomaly-shelving",
        "settings-alerts"
    ],
    ButtonPermissions: ["anomalies-export", "settings-alerts-save"]);
```

```ts
export const menuPermissions = {
  workloadDashboard: 'workload-dashboard',
  anomalyDashboard: 'anomaly-dashboard',
  workloadOutboundDetail: 'workload-outbound-detail',
  workloadSkuDetail: 'workload-sku-detail',
  workloadCartonDetail: 'workload-carton-detail',
  workloadFutureInboundVolume: 'workload-future-inbound-volume',
  anomalyOutbound: 'anomaly-outbound',
  anomalyInbound: 'anomaly-inbound',
  anomalyShelving: 'anomaly-shelving',
  settingsAlerts: 'settings-alerts',
} as const
```

- [ ] **Step 4: Re-run the auth test**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~Auth_permissions_returns_workload_and_anomaly_menu_permissions"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add backend/Api/Controllers/AuthController.cs backend/Api.Tests/AuthControllerTests.cs frontend/src/shared/constants/permissions.ts
git commit -m "feat: add workload dashboard permission codes"
```

### Task 2: Define Workload DTOs And Endpoint Tests

**Files:**
- Create: `backend/Contracts/Workload/WorkloadDtos.cs`
- Create: `backend/Api.Tests/WorkloadControllerTests.cs`

- [ ] **Step 1: Write failing workload endpoint tests**

```csharp
[Fact]
public async Task Workload_dashboard_returns_expected_sections_for_selected_warehouse()
{
    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

    var response = await client.GetAsync("/api/workloads/dashboard?warehouseCode=DE");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<ApiResponse<WorkloadDashboardResponse>>();
    Assert.Equal("DE", result!.Data!.Warehouse.Code);
    Assert.Equal(3, result.Data.OverdueTasks.Count);
    Assert.Equal(4, result.Data.TodayInbound.Count);
    Assert.Equal(7, result.Data.FutureInboundForecast.Count);
}

[Fact]
public async Task Workload_future_inbound_volume_returns_truck_and_sea_specific_columns()
{
    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

    var response = await client.GetAsync("/api/workloads/future-inbound-volume?warehouseCode=ON&dateStart=2026-05-08&dateEnd=2026-05-16");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<ApiResponse<FutureInboundVolumeResponse>>();
    Assert.NotEmpty(result!.Data!.Items);
    Assert.Contains(result.Data.Items, item => item.TransportMode == "sea" && item.SeaContainerCount >= 0);
    Assert.Contains(result.Data.Items, item => item.TransportMode == "truck" && item.TruckPalletCount >= 0);
}
```

- [ ] **Step 2: Run the workload tests and confirm they fail with 404**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~WorkloadControllerTests"`

Expected: FAIL because `/api/workloads/*` does not exist yet.

- [ ] **Step 3: Create the workload DTO file with explicit contracts**

```csharp
namespace OpsMonitor.Contracts.Workload;

public sealed record WarehouseOption(string Code, string Name);
public sealed record StatusSplit(int Total, int Processed, int Pending);
public sealed record MetricSplit(decimal Total, decimal Processed, decimal Pending);
public sealed record TodayInboundSummary(
    MetricSplit TotalCartons,
    MetricSplit TotalWeightKg,
    MetricSplit TotalVolumeM3,
    MetricSplit TotalUnits,
    MetricSplit TotalSkuCount);
public sealed record InboundTransportRow(
    string TransportMode,
    MetricSplit Cartons,
    MetricSplit WeightKg,
    MetricSplit VolumeM3,
    MetricSplit Units,
    MetricSplit SkuCount);
public sealed record TomorrowInboundSummary(
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int TotalUnits,
    int TotalSkuCount);
public sealed record TomorrowInboundTransportRow(
    string TransportMode,
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int TotalUnits,
    int TotalSkuCount);
public sealed record OverdueTaskCard(string Code, string Title, int Total);
public sealed record FutureInboundForecastBar(
    string ArrivalDate,
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int SeaContainerCount,
    int TruckPalletCount);
```

```csharp
public sealed record WorkloadDashboardResponse(
    WarehouseOption Warehouse,
    IReadOnlyList<OverdueTaskCard> OverdueTasks,
    StatusSplit TodayOutboundPackages,
    StatusSplit TodayShelvingSkus,
    TodayInboundSummary TodayInboundSummary,
    IReadOnlyList<InboundTransportRow> TodayInbound,
    TomorrowInboundSummary TomorrowInboundSummary,
    IReadOnlyList<TomorrowInboundTransportRow> TomorrowInbound,
    IReadOnlyList<FutureInboundForecastBar> FutureInboundForecast);
```

```csharp
public sealed record OutboundPackageDetailItem(
    string OrderId,
    string Customer,
    string LogisticsProvider,
    string TrackingNo,
    string ProductService,
    string ShippingRule,
    DateTimeOffset OrderTime,
    DateTimeOffset DeadlineAt,
    DateTimeOffset? ActualOutboundTime,
    string ProcessingStatus);

public sealed record OutboundPackageDetailResponse(
    StatusSplit Summary,
    IReadOnlyList<OutboundPackageDetailItem> Items);
```

```csharp
public sealed record SkuDetailItem(
    string InventoryCode,
    string Sku,
    string Customer,
    string CartonId,
    string AsnId,
    string TransportMode,
    int TotalUnits,
    int ProcessedUnits,
    int PendingUnits,
    DateTimeOffset ArrivalTime,
    DateTimeOffset DeadlineAt,
    DateTimeOffset? ActualShelvedAt,
    string ProcessingStatus);

public sealed record SkuDetailResponse(
    StatusSplit Summary,
    IReadOnlyList<SkuDetailItem> Items);

public sealed record CartonDetailSummary(
    int Total,
    int Processed,
    int Pending,
    double TotalWeightKg,
    double TotalVolumeM3);

public sealed record CartonDetailItem(
    string CartonId,
    string AsnId,
    string TransportMode,
    DateTimeOffset EtaAt,
    DateTimeOffset? ArrivedAt,
    double TotalWeightKg,
    double TotalVolumeM3,
    int TotalUnits,
    int TotalSkuCount,
    string ProcessingStatus);

public sealed record CartonDetailResponse(
    CartonDetailSummary Summary,
    IReadOnlyList<CartonDetailItem> Items);

public sealed record FutureInboundVolumeSummary(
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int SeaContainerCount,
    int TruckPalletCount);

public sealed record FutureInboundVolumeItem(
    string ArrivalDate,
    string TransportMode,
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int TotalUnits,
    int TotalSkuCount,
    int? TruckPalletCount,
    int? SeaContainerCount);

public sealed record FutureInboundVolumeResponse(
    FutureInboundVolumeSummary Summary,
    IReadOnlyList<FutureInboundVolumeItem> Items);
```

- [ ] **Step 4: Re-run the tests to verify they now fail on missing controller implementation instead of missing types**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~WorkloadControllerTests"`

Expected: FAIL with 404 or empty responses, not compiler errors.

- [ ] **Step 5: Commit**

```bash
git add backend/Contracts/Workload/WorkloadDtos.cs backend/Api.Tests/WorkloadControllerTests.cs
git commit -m "test: define workload contracts and endpoint expectations"
```

### Task 3: Implement The Workload Controller

**Files:**
- Create: `backend/Api/Controllers/WorkloadsController.cs`
- Modify: `backend/Contracts/Workload/WorkloadDtos.cs`
- Test: `backend/Api.Tests/WorkloadControllerTests.cs`

- [ ] **Step 1: Write the controller skeleton with all five authorized routes**

```csharp
[ApiController]
[Route("api/workloads")]
[Authorize]
public class WorkloadsController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetDashboard([FromQuery] string warehouseCode = "DE") =>
        throw new NotImplementedException();

    [HttpGet("outbound-packages")]
    public IActionResult GetOutboundPackages(
        [FromQuery] string warehouseCode = "DE",
        [FromQuery] DateTimeOffset? dateStart = null,
        [FromQuery] DateTimeOffset? dateEnd = null,
        [FromQuery] string processingStatus = "all",
        [FromQuery] string? keyword = null) =>
        throw new NotImplementedException();

    [HttpGet("skus")]
    public IActionResult GetSkus(
        [FromQuery] string warehouseCode = "DE",
        [FromQuery] DateTimeOffset? dateStart = null,
        [FromQuery] DateTimeOffset? dateEnd = null,
        [FromQuery] string processingStatus = "all",
        [FromQuery] string? transportMode = null,
        [FromQuery] string? keyword = null) =>
        throw new NotImplementedException();

    [HttpGet("cartons")]
    public IActionResult GetCartons(
        [FromQuery] string warehouseCode = "DE",
        [FromQuery] DateTimeOffset? dateStart = null,
        [FromQuery] DateTimeOffset? dateEnd = null,
        [FromQuery] string processingStatus = "all",
        [FromQuery] string? transportMode = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string context = "today") =>
        throw new NotImplementedException();

    [HttpGet("future-inbound-volume")]
    public IActionResult GetFutureInboundVolume(
        [FromQuery] string warehouseCode = "DE",
        [FromQuery] DateTimeOffset? dateStart = null,
        [FromQuery] DateTimeOffset? dateEnd = null,
        [FromQuery] string? transportMode = null) =>
        throw new NotImplementedException();
}
```

- [ ] **Step 2: Implement shared filtering helpers**

```csharp
private static WarehouseOption ResolveWarehouse(string warehouseCode) =>
    warehouseCode == "ON"
        ? new("ON", "安大略仓")
        : new("DE", "德国仓");

private static bool MatchStatus(string actualStatus, string processingStatus) =>
    processingStatus switch
    {
        "processed" => actualStatus == "processed",
        "pending" => actualStatus == "pending",
        _ => true,
    };

private static bool MatchTransport(string actualMode, string? transportMode) =>
    string.IsNullOrWhiteSpace(transportMode) || actualMode == transportMode;
```

- [ ] **Step 3: Return mock payloads that match the approved design**

```csharp
var overdueTasks = new[]
{
    new OverdueTaskCard("outbound", "包裹出库异常", 12),
    new OverdueTaskCard("inbound", "入库单箱子到仓不齐", 7),
    new OverdueTaskCard("shelving", "SKU上架异常", 9),
};

var futureBars = Enumerable.Range(1, 7)
    .Select(offset => new FutureInboundForecastBar(
        ArrivalDate: DateTime.Today.AddDays(offset).ToString("yyyy-MM-dd"),
        TotalCartons: 80 + offset * 6,
        TotalWeightKg: 1200 + offset * 55,
        TotalVolumeM3: 18 + offset * 0.9,
        SeaContainerCount: offset % 3 == 0 ? 2 : 1,
        TruckPalletCount: offset % 2 == 0 ? 8 : 4))
    .ToList();
```

```csharp
var items = rawItems
    .Where(item => MatchStatus(item.ProcessingStatus, processingStatus))
    .Where(item => MatchTransport(item.TransportMode, transportMode))
    .Where(item => string.IsNullOrWhiteSpace(keyword) || item.OrderId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
    .ToList();
```

- [ ] **Step 4: Run the workload controller tests**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~WorkloadControllerTests"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add backend/Api/Controllers/WorkloadsController.cs backend/Contracts/Workload/WorkloadDtos.cs backend/Api.Tests/WorkloadControllerTests.cs
git commit -m "feat: add workload dashboard and detail mock endpoints"
```

### Task 4: Add Warehouse Context To Existing Anomaly APIs

**Files:**
- Create: `backend/Api.Tests/DashboardControllerTests.cs`
- Create: `backend/Api.Tests/AnomaliesControllerTests.cs`
- Modify: `backend/Api/Controllers/DashboardController.cs`
- Modify: `backend/Api/Controllers/AnomaliesController.cs`

- [ ] **Step 1: Write failing tests for warehouse-bound anomaly requests**

```csharp
[Fact]
public async Task Dashboard_returns_selected_warehouse_from_query()
{
    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

    var response = await client.GetAsync("/api/dashboard?warehouseCode=ON");

    var result = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardResponse>>();
    Assert.Equal("ON", result!.Data!.Warehouse.WarehouseId);
}

[Fact]
public async Task Outbound_anomalies_accept_overdue_and_warehouse_query()
{
    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

    var response = await client.GetAsync("/api/anomalies/outbound?warehouseCode=DE&riskStatus=overdue");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

- [ ] **Step 2: Run the controller tests and confirm the warehouse assertions fail**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~DashboardControllerTests|FullyQualifiedName~AnomaliesControllerTests"`

Expected: FAIL because the controllers ignore `warehouseCode`.

- [ ] **Step 3: Add `warehouseCode` query handling to the anomaly controllers**

```csharp
public IActionResult Get([FromQuery] string warehouseCode = "DE")
{
    var warehouse = warehouseCode == "ON"
        ? new WarehouseInfo("ON", "安大略仓")
        : new WarehouseInfo("DE", "德国仓");

    var baseStatus = new BaseStatus(
        LastSyncTime: lastSync,
        SyncStatus: delayedDataFlag ? "delayed" : "success",
        TodayReminderCount: 16,
        TodayOverdueCount: 9,
        AlertChannels:
        [
            new("wechat", "企业微信", "healthy"),
            new("email", "邮件", "healthy"),
        ],
        DelayedDataFlag: delayedDataFlag,
        DelayedReason: delayedDataFlag ? "业务系统同步超时" : null);

    var todayOverview = new TodayOverview(
        OutboundRiskCount: 23,
        InboundRiskCount: 7,
        ShelvingRiskCount: 15,
        TodayVolumePressureLevel: "medium",
        OperationTip: "今天主要风险集中在出库时效，建议优先处理渠道 A 的待出库订单。");

    var anomalyPreview = new AnomalyPreview(
        Outbound: new(23, 10, 13, outboundItems),
        Inbound: new(7, 2, 5, inboundItems),
        Shelving: new(15, 4, 11, shelvingItems));

    var data = new DashboardResponse(
        Warehouse: warehouse,
        BaseStatus: baseStatus,
        TodayOverview: todayOverview,
        AnomalyPreview: anomalyPreview,
        Forecast7Days: forecast7Days);

    return Ok(ApiResponse<DashboardResponse>.Ok(data));
}
```

```csharp
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
    var warehouseSeed = warehouseCode == "ON" ? 200 : 100;
    var items = Enumerable.Range(1, 5).Select(i => new OutboundItem(
        OrderId: $"SO{warehouseSeed + i}",
        CustomerOrChannel: i % 2 == 0 ? "客户A / 渠道A" : "客户B / 渠道B",
        OrderTime: now.AddHours(-i * 6),
        DeadlineAt: now.AddHours(i % 3 == 0 ? -2 : 2),
        TimeStatus: i % 3 == 0 ? "overdue" : "remaining",
        TimeValueMinutes: i % 3 == 0 ? 120 : 180,
        TimeValueLabel: i % 3 == 0 ? "超时 2 小时" : $"剩余 {3 * i} 小时",
        CurrentStatus: "待出库",
        Shipped: false,
        RiskStatus: i % 3 == 0 ? "overdue" : "imminent"));

    if (!string.IsNullOrWhiteSpace(riskStatus))
    {
        items = items.Where(item => item.RiskStatus == riskStatus);
    }

    var data = new OutboundResponse(
        Summary: new(128, 48, 80),
        FilterOptions: new(["渠道A", "渠道B"], ["客户A", "客户B"]),
        List: new(pageNo, pageSize, 128, items.ToList()));

    return Ok(ApiResponse<OutboundResponse>.Ok(data));
}
```

- [ ] **Step 4: Re-run the anomaly/dashboard backend tests**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj --filter "FullyQualifiedName~DashboardControllerTests|FullyQualifiedName~AnomaliesControllerTests"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add backend/Api/Controllers/DashboardController.cs backend/Api/Controllers/AnomaliesController.cs backend/Api.Tests/DashboardControllerTests.cs backend/Api.Tests/AnomaliesControllerTests.cs
git commit -m "feat: add warehouse-aware anomaly endpoints"
```

### Task 5: Add Frontend Workload Types, API Clients, And Export Utilities

**Files:**
- Modify: `frontend/src/services/types.ts`
- Modify: `frontend/src/services/api.ts`
- Create: `frontend/src/modules/workload/utils/datePresets.ts`
- Create: `frontend/src/modules/workload/utils/exportCsv.ts`
- Create: `frontend/src/modules/workload/utils/formatters.ts`
- Create: `frontend/src/modules/workload/__tests__/exportCsv.spec.ts`

- [ ] **Step 1: Write the failing CSV utility test**

```ts
import { describe, expect, it } from 'vitest'
import { buildCsv } from '@/modules/workload/utils/exportCsv'

describe('buildCsv', () => {
  it('renders header and rows in CSV order', () => {
    const csv = buildCsv(
      ['订单号', '客户'],
      [
        ['SO1001', '客户A'],
        ['SO1002', '客户B'],
      ],
    )

    expect(csv).toContain('订单号,客户')
    expect(csv).toContain('SO1001,客户A')
    expect(csv).toContain('SO1002,客户B')
  })
})
```

- [ ] **Step 2: Run the frontend test and confirm it fails**

Run: `npm --prefix frontend test -- --runInBand`

Expected: FAIL because `buildCsv` does not exist yet.

- [ ] **Step 3: Add workload interfaces and API methods**

```ts
export interface StatusSplit {
  total: number
  processed: number
  pending: number
}

export interface MetricSplit {
  total: number
  processed: number
  pending: number
}

export interface InboundTransportRow {
  transportMode: 'sea' | 'air' | 'express' | 'truck'
  cartons: MetricSplit
  weightKg: MetricSplit
  volumeM3: MetricSplit
  units: MetricSplit
  skuCount: MetricSplit
}

export interface TomorrowInboundTransportRow {
  transportMode: 'sea' | 'air' | 'express' | 'truck'
  totalCartons: number
  totalWeightKg: number
  totalVolumeM3: number
  totalUnits: number
  totalSkuCount: number
}

export interface FutureInboundForecastBar {
  arrivalDate: string
  totalCartons: number
  totalWeightKg: number
  totalVolumeM3: number
  seaContainerCount: number
  truckPalletCount: number
}

export interface OutboundPackageDetailItem {
  orderId: string
  customer: string
  logisticsProvider: string
  trackingNo: string
  productService: string
  shippingRule: string
  orderTime: string
  deadlineAt: string
  actualOutboundTime?: string
  processingStatus: 'processed' | 'pending'
}

export interface SkuDetailItem {
  inventoryCode: string
  sku: string
  customer: string
  cartonId: string
  asnId: string
  transportMode: 'sea' | 'air' | 'express' | 'truck'
  totalUnits: number
  processedUnits: number
  pendingUnits: number
  arrivalTime: string
  deadlineAt: string
  actualShelvedAt?: string
  processingStatus: 'processed' | 'pending'
}

export interface CartonDetailItem {
  cartonId: string
  asnId: string
  transportMode: 'sea' | 'air' | 'express' | 'truck'
  etaAt: string
  arrivedAt?: string
  totalWeightKg: number
  totalVolumeM3: number
  totalUnits: number
  totalSkuCount: number
  processingStatus: 'processed' | 'pending'
}

export interface FutureInboundVolumeItem {
  arrivalDate: string
  transportMode: 'sea' | 'air' | 'express' | 'truck'
  totalCartons: number
  totalWeightKg: number
  totalVolumeM3: number
  totalUnits: number
  totalSkuCount: number
  truckPalletCount: number | null
  seaContainerCount: number | null
}

export interface WorkloadDashboardData {
  warehouse: { code: string; name: string }
  overdueTasks: Array<{ code: string; title: string; total: number }>
  todayOutboundPackages: StatusSplit
  todayShelvingSkus: StatusSplit
  todayInboundSummary: {
    totalCartons: MetricSplit
    totalWeightKg: MetricSplit
    totalVolumeM3: MetricSplit
    totalUnits: MetricSplit
    totalSkuCount: MetricSplit
  }
  todayInbound: InboundTransportRow[]
  tomorrowInboundSummary: {
    totalCartons: number
    totalWeightKg: number
    totalVolumeM3: number
    totalUnits: number
    totalSkuCount: number
  }
  tomorrowInbound: TomorrowInboundTransportRow[]
  futureInboundForecast: FutureInboundForecastBar[]
}

export interface WorkloadBaseQuery {
  warehouseCode: string
  dateStart: string
  dateEnd: string
}

export interface WorkloadOutboundQuery extends WorkloadBaseQuery {
  processingStatus: 'all' | 'processed' | 'pending'
  keyword?: string
}

export interface WorkloadSkuQuery extends WorkloadBaseQuery {
  processingStatus: 'all' | 'processed' | 'pending'
  transportMode?: 'sea' | 'air' | 'express' | 'truck'
  keyword?: string
}

export interface WorkloadCartonQuery extends WorkloadBaseQuery {
  processingStatus: 'all' | 'processed' | 'pending'
  transportMode?: 'sea' | 'air' | 'express' | 'truck'
  keyword?: string
  context: 'today' | 'tomorrow'
}

export interface FutureInboundVolumeQuery extends WorkloadBaseQuery {
  transportMode?: 'sea' | 'air' | 'express' | 'truck'
}

export interface OutboundPackageDetailData {
  summary: StatusSplit
  items: OutboundPackageDetailItem[]
}

export interface SkuDetailData {
  summary: StatusSplit
  items: SkuDetailItem[]
}

export interface CartonDetailData {
  summary: {
    total: number
    processed: number
    pending: number
    totalWeightKg: number
    totalVolumeM3: number
  }
  items: CartonDetailItem[]
}

export interface FutureInboundVolumeData {
  summary: {
    totalCartons: number
    totalWeightKg: number
    totalVolumeM3: number
    seaContainerCount: number
    truckPalletCount: number
  }
  items: FutureInboundVolumeItem[]
}
```

```ts
export const workloadApi = {
  dashboard: (warehouseCode: string) =>
    http.get<ApiResponse<WorkloadDashboardData>>('/workloads/dashboard', { params: { warehouseCode } }),
  outboundPackages: (params: WorkloadOutboundQuery) =>
    http.get<ApiResponse<OutboundPackageDetailData>>('/workloads/outbound-packages', { params }),
  skus: (params: WorkloadSkuQuery) =>
    http.get<ApiResponse<SkuDetailData>>('/workloads/skus', { params }),
  cartons: (params: WorkloadCartonQuery) =>
    http.get<ApiResponse<CartonDetailData>>('/workloads/cartons', { params }),
  futureInboundVolume: (params: FutureInboundVolumeQuery) =>
    http.get<ApiResponse<FutureInboundVolumeData>>('/workloads/future-inbound-volume', { params }),
}
```

```ts
export function getDatePresetRange(preset: 'today' | 'tomorrow' | 'future7days') {
  const now = new Date()
  const start = new Date(now)
  const end = new Date(now)

  if (preset === 'tomorrow') {
    start.setDate(start.getDate() + 1)
    end.setDate(end.getDate() + 1)
  }

  if (preset === 'future7days') {
    start.setDate(start.getDate() + 1)
    end.setDate(end.getDate() + 7)
  }

  return {
    dateStart: start.toISOString().slice(0, 10),
    dateEnd: end.toISOString().slice(0, 10),
  }
}
```

```ts
export function buildCsv(headers: string[], rows: Array<Array<string | number | null | undefined>>) {
  const escape = (value: string | number | null | undefined) =>
    `"${String(value ?? '').replaceAll('"', '""')}"`

  return [headers.map(escape).join(','), ...rows.map(row => row.map(escape).join(','))].join('\n')
}

export function downloadCsv(options: {
  fileName: string
  headers: string[]
  rows: Array<Array<string | number | null | undefined>>
}) {
  const csv = buildCsv(options.headers, options.rows)
  const blob = new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = options.fileName
  anchor.click()
  URL.revokeObjectURL(url)
}
```

- [ ] **Step 4: Extend anomaly query types with warehouse support**

```ts
export interface OutboundQuery {
  warehouseCode?: string
  riskStatus?: string
  channel?: string
  customer?: string
  orderTimeStart?: string
  orderTimeEnd?: string
  pageNo: number
  pageSize: number
  sortBy?: string
  sortDirection?: string
}

export interface InboundQuery {
  warehouseCode?: string
  riskStatus?: string
  etaDateStart?: string
  etaDateEnd?: string
  pageNo: number
  pageSize: number
  sortBy?: string
  sortDirection?: string
}

export interface ShelvingQuery {
  warehouseCode?: string
  riskStatus?: string
  arrivalDateStart?: string
  arrivalDateEnd?: string
  pageNo: number
  pageSize: number
  sortBy?: string
  sortDirection?: string
}
```

- [ ] **Step 5: Re-run the frontend tests**

Run: `npm --prefix frontend test -- --runInBand`

Expected: PASS for `exportCsv.spec.ts` and existing router guard tests.

- [ ] **Step 6: Commit**

```bash
git add frontend/src/services/types.ts frontend/src/services/api.ts frontend/src/modules/workload/utils/datePresets.ts frontend/src/modules/workload/utils/exportCsv.ts frontend/src/modules/workload/utils/formatters.ts frontend/src/modules/workload/__tests__/exportCsv.spec.ts
git commit -m "feat: add workload frontend service contracts"
```

### Task 6: Wire Routes, Menu, Tabs, And Default Navigation

**Files:**
- Create: `frontend/src/modules/workload/__tests__/workload-routes.spec.ts`
- Modify: `frontend/src/router/routes.ts`
- Modify: `frontend/src/router/index.ts`
- Modify: `frontend/src/layouts/AppShell.vue`
- Modify: `frontend/src/stores/tabs.ts`

- [ ] **Step 1: Write the failing route/navigation tests**

```ts
import { beforeEach, describe, expect, it } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { createAppRouter } from '@/router'

describe('workload routes', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('registers the anomaly dashboard route', async () => {
    const router = createAppRouter()
    const match = router.resolve('/anomaly-dashboard')
    expect(match.name).toBe('anomaly-dashboard')
  })
})
```

- [ ] **Step 2: Run the route tests and confirm they fail**

Run: `npm --prefix frontend test -- --runInBand`

Expected: FAIL because `/anomaly-dashboard` does not exist yet.

- [ ] **Step 3: Register the new routes**

```ts
{
  path: 'dashboard',
  name: 'dashboard',
  component: WorkloadDashboardView,
  meta: { title: '工作量看板', permission: 'workload-dashboard', requiresAuth: true },
},
{
  path: 'anomaly-dashboard',
  name: 'anomaly-dashboard',
  component: DashboardView,
  meta: { title: '异常看板', permission: 'anomaly-dashboard', requiresAuth: true },
},
{
  path: 'workloads/outbound-packages',
  name: 'workload-outbound-detail',
  component: OutboundPackagesDetailView,
  meta: { title: '出库包裹详情页', permission: 'workload-outbound-detail', requiresAuth: true },
},
{
  path: 'workloads/skus',
  name: 'workload-sku-detail',
  component: SkuDetailView,
  meta: { title: 'SKU详情页', permission: 'workload-sku-detail', requiresAuth: true },
},
{
  path: 'workloads/cartons',
  name: 'workload-carton-detail',
  component: CartonDetailView,
  meta: { title: '箱子详情页', permission: 'workload-carton-detail', requiresAuth: true },
},
{
  path: 'workloads/future-inbound-volume',
  name: 'workload-future-inbound-volume',
  component: FutureInboundVolumeDetailView,
  meta: { title: '待到仓货量详情页', permission: 'workload-future-inbound-volume', requiresAuth: true },
},
```

- [ ] **Step 4: Update menu and tabs**

```ts
const menuGroups: MenuItem[] = [
  { label: '工作量看板', path: '/dashboard', icon: '◧' },
  { label: '异常看板', path: '/anomaly-dashboard', icon: '◨' },
  {
    label: '异常管理',
    key: '/anomalies',
    icon: '◩',
    children: [
      { label: '出库异常', path: '/anomalies/outbound' },
      { label: '到仓不齐', path: '/anomalies/inbound' },
      { label: '上架异常', path: '/anomalies/shelving' },
    ],
  },
]
```

```ts
const tabs = ref<TabItem[]>([
  { path: '/dashboard', title: '工作量看板', closeable: false },
])
```

- [ ] **Step 5: Re-run frontend route tests**

Run: `npm --prefix frontend test -- --runInBand`

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add frontend/src/router/routes.ts frontend/src/router/index.ts frontend/src/layouts/AppShell.vue frontend/src/stores/tabs.ts frontend/src/modules/workload/__tests__/workload-routes.spec.ts
git commit -m "feat: wire workload dashboard navigation"
```

### Task 7: Implement The Workload Dashboard View

**Files:**
- Create: `frontend/src/modules/workload/components/FutureInboundForecastChart.vue`
- Create: `frontend/src/modules/workload/components/InboundMetricsMatrix.vue`
- Create: `frontend/src/modules/workload/views/WorkloadDashboardView.vue`
- Modify: `frontend/src/services/api.ts`

- [ ] **Step 1: Build the forecast chart wrapper**

```vue
<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import * as echarts from 'echarts'
import type { FutureInboundForecastBar } from '@/services/types'

const props = defineProps<{ items: FutureInboundForecastBar[] }>()
const root = ref<HTMLDivElement | null>(null)
let chart: echarts.ECharts | null = null

function render() {
  if (!root.value) return
  chart ??= echarts.init(root.value)
  chart.setOption({
    tooltip: {
      trigger: 'axis',
      formatter: (params: any[]) => {
        const item = params[0].data
        return [
          item.arrivalDate,
          `总箱数：${item.totalCartons}`,
          `总重量：${item.totalWeightKg}`,
          `总体积：${item.totalVolumeM3}`,
          `海运柜数：${item.seaContainerCount}`,
          `卡派板数：${item.truckPalletCount}`,
        ].join('<br/>')
      },
    },
    xAxis: { type: 'category', data: props.items.map(item => item.arrivalDate) },
    yAxis: { type: 'value', name: '总箱数' },
    series: [{ type: 'bar', data: props.items }],
  })
}
</script>
```

- [ ] **Step 2: Build the inbound metrics matrix**

```vue
<script setup lang="ts">
defineProps<{
  title: string
  showStatusSplit: boolean
  rows: InboundTransportRow[] | TomorrowInboundTransportRow[]
}>()
</script>
```

```vue
<el-table :data="rows" size="small" border>
  <el-table-column prop="transportMode" label="货运方式" width="120" />
  <el-table-column label="总箱数 / 已处理 / 待处理">
    <template #default="{ row }">
      <span v-if="showStatusSplit">{{ row.cartons.total }} / {{ row.cartons.processed }} / {{ row.cartons.pending }}</span>
      <span v-else>{{ row.totalCartons }}</span>
    </template>
  </el-table-column>
</el-table>
```

- [ ] **Step 3: Implement the dashboard page using the approved module order**

```vue
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { workloadApi } from '@/services/api'
import { getDatePresetRange } from '@/modules/workload/utils/datePresets'

const router = useRouter()
const warehouseCode = ref<'DE' | 'ON'>('DE')
const loading = ref(true)
const data = ref<WorkloadDashboardData | null>(null)

async function load() {
  loading.value = true
  try {
    const res = await workloadApi.dashboard(warehouseCode.value)
    data.value = res.data.data ?? null
  } finally {
    loading.value = false
  }
}

function openOutboundDetail() {
  router.push({ name: 'workload-outbound-detail', query: { warehouseCode: warehouseCode.value, ...getDatePresetRange('today') } })
}
</script>
```

```vue
<template>
  <div class="workload-dashboard">
    <div class="toolbar">
      <el-radio-group v-model="warehouseCode" @change="load">
        <el-radio-button label="DE">德国仓</el-radio-button>
        <el-radio-button label="ON">安大略仓</el-radio-button>
      </el-radio-group>
    </div>

    <section>
      <header><h3>已超时待处理任务</h3></header>
    </section>

    <section>
      <header><h3>今天待处理任务</h3></header>
    </section>

    <section>
      <header><h3>明天待处理任务</h3></header>
    </section>

    <section>
      <header><h3>未来7天待到仓货量预估</h3></header>
    </section>
  </div>
</template>
```

- [ ] **Step 4: Wire all approved jumps from the dashboard**

```ts
function openSkuDetail() {
  router.push({ name: 'workload-sku-detail', query: { warehouseCode: warehouseCode.value, ...getDatePresetRange('today') } })
}

function openCartonDetail(context: 'today' | 'tomorrow') {
  router.push({
    name: 'workload-carton-detail',
    query: {
      warehouseCode: warehouseCode.value,
      context,
      ...getDatePresetRange(context),
    },
  })
}

function openOverdueAnomaly(name: 'anomaly-outbound' | 'anomaly-inbound' | 'anomaly-shelving') {
  router.push({ name, query: { warehouseCode: warehouseCode.value, riskStatus: 'overdue' } })
}
```

- [ ] **Step 5: Run a frontend build**

Run: `npm --prefix frontend run build`

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add frontend/src/modules/workload/components/FutureInboundForecastChart.vue frontend/src/modules/workload/components/InboundMetricsMatrix.vue frontend/src/modules/workload/views/WorkloadDashboardView.vue
git commit -m "feat: add workload dashboard homepage"
```

### Task 8: Implement The Outbound And SKU Detail Pages

**Files:**
- Create: `frontend/src/modules/workload/views/OutboundPackagesDetailView.vue`
- Create: `frontend/src/modules/workload/views/SkuDetailView.vue`
- Modify: `frontend/src/modules/workload/utils/exportCsv.ts`
- Modify: `frontend/src/services/api.ts`

- [ ] **Step 1: Implement the outbound detail page with route-driven defaults**

```vue
<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { workloadApi } from '@/services/api'
import { downloadCsv } from '@/modules/workload/utils/exportCsv'

const route = useRoute()
const query = reactive({
  warehouseCode: String(route.query.warehouseCode ?? 'DE'),
  dateStart: String(route.query.dateStart ?? ''),
  dateEnd: String(route.query.dateEnd ?? ''),
  processingStatus: String(route.query.processingStatus ?? 'all'),
  keyword: String(route.query.keyword ?? ''),
})
</script>
```

```vue
<el-table :data="rows" size="small" border>
  <el-table-column prop="orderId" label="订单号" width="160" />
  <el-table-column prop="customer" label="客户" width="140" />
  <el-table-column prop="logisticsProvider" label="物流商" width="140" />
  <el-table-column prop="trackingNo" label="挂号" width="160" />
  <el-table-column prop="productService" label="产品服务" width="140" />
  <el-table-column prop="shippingRule" label="发货规则" width="140" />
  <el-table-column prop="processingStatus" label="处理状态" width="110" />
</el-table>
```

- [ ] **Step 2: Implement the SKU detail page with the approved columns**

```vue
<el-table :data="rows" size="small" border>
  <el-table-column prop="inventoryCode" label="库存编码" width="140" />
  <el-table-column prop="sku" label="SKU" width="140" />
  <el-table-column prop="customer" label="客户" width="120" />
  <el-table-column prop="cartonId" label="所属箱号" width="140" />
  <el-table-column prop="asnId" label="所属入库单号" width="160" />
  <el-table-column prop="transportModeLabel" label="货运方式" width="110" />
  <el-table-column prop="totalUnits" label="总件数" width="90" align="right" />
  <el-table-column prop="processedUnits" label="已上架数量" width="110" align="right" />
  <el-table-column prop="pendingUnits" label="待上架数量" width="110" align="right" />
</el-table>
```

- [ ] **Step 3: Reuse the CSV helper for both pages**

```ts
downloadCsv({
  fileName: `outbound-packages-${query.warehouseCode}-${query.dateStart}-${query.dateEnd}.csv`,
  headers: ['订单号', '客户', '物流商', '挂号', '产品服务', '发货规则', '下单时间', '应完成出库时间', '实际出库时间', '处理状态'],
  rows: rows.value.map(item => [
    item.orderId,
    item.customer,
    item.logisticsProvider,
    item.trackingNo,
    item.productService,
    item.shippingRule,
    item.orderTime,
    item.deadlineAt,
    item.actualOutboundTime ?? '',
    item.processingStatus,
  ]),
})
```

- [ ] **Step 4: Run the frontend build**

Run: `npm --prefix frontend run build`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/modules/workload/views/OutboundPackagesDetailView.vue frontend/src/modules/workload/views/SkuDetailView.vue frontend/src/modules/workload/utils/exportCsv.ts
git commit -m "feat: add outbound and sku workload detail pages"
```

### Task 9: Implement The Carton And Future Inbound Detail Pages

**Files:**
- Create: `frontend/src/modules/workload/views/CartonDetailView.vue`
- Create: `frontend/src/modules/workload/views/FutureInboundVolumeDetailView.vue`
- Modify: `frontend/src/modules/workload/utils/formatters.ts`

- [ ] **Step 1: Implement the shared carton detail page**

```vue
<script setup lang="ts">
const route = useRoute()
const context = computed(() => String(route.query.context ?? 'today') as 'today' | 'tomorrow')
</script>
```

```vue
<div class="summary-strip">
  <template v-if="context === 'today'">
    <div>总箱数：{{ summary.total }}</div>
    <div>已处理：{{ summary.processed }}</div>
    <div>待处理：{{ summary.pending }}</div>
  </template>
  <template v-else>
    <div>总箱数：{{ summary.total }}</div>
    <div>总重量(kg)：{{ summary.totalWeightKg }}</div>
    <div>总体积(m3)：{{ summary.totalVolumeM3 }}</div>
  </template>
</div>
```

```vue
<el-table :data="rows" size="small" border>
  <el-table-column prop="cartonId" label="箱号" width="160" />
  <el-table-column prop="asnId" label="入库单号" width="160" />
  <el-table-column prop="transportModeLabel" label="货运方式" width="110" />
  <el-table-column prop="etaAt" label="预计到仓时间" width="180" />
  <el-table-column prop="arrivedAt" label="实际到仓时间" width="180" />
  <el-table-column prop="totalWeightKg" label="总重量(kg)" width="110" align="right" />
  <el-table-column prop="totalVolumeM3" label="总体积(m3)" width="110" align="right" />
  <el-table-column prop="totalUnits" label="总件数" width="90" align="right" />
  <el-table-column prop="totalSkuCount" label="总SKU数" width="100" align="right" />
</el-table>
```

- [ ] **Step 2: Implement the future inbound volume detail page**

```vue
<div class="summary-strip">
  <div>总箱数：{{ summary.totalCartons }}</div>
  <div>总重量(kg)：{{ summary.totalWeightKg }}</div>
  <div>总体积(m3)：{{ summary.totalVolumeM3 }}</div>
  <div>海运柜数：{{ summary.seaContainerCount }}</div>
  <div>卡派板数：{{ summary.truckPalletCount }}</div>
</div>
```

```vue
<el-table :data="rows" size="small" border>
  <el-table-column prop="arrivalDate" label="到货日期" width="120" />
  <el-table-column prop="transportModeLabel" label="货运方式" width="110" />
  <el-table-column prop="totalCartons" label="总箱数" width="90" align="right" />
  <el-table-column prop="totalWeightKg" label="总重量(kg)" width="110" align="right" />
  <el-table-column prop="totalVolumeM3" label="总体积(m3)" width="110" align="right" />
  <el-table-column prop="totalUnits" label="总件数" width="90" align="right" />
  <el-table-column prop="totalSkuCount" label="总SKU数" width="100" align="right" />
  <el-table-column label="卡派板数" width="100" align="right">
    <template #default="{ row }">{{ row.transportMode === 'truck' ? row.truckPalletCount : '-' }}</template>
  </el-table-column>
  <el-table-column label="海运柜数" width="100" align="right">
    <template #default="{ row }">{{ row.transportMode === 'sea' ? row.seaContainerCount : '-' }}</template>
  </el-table-column>
</el-table>
```

- [ ] **Step 3: Run the frontend build**

Run: `npm --prefix frontend run build`

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add frontend/src/modules/workload/views/CartonDetailView.vue frontend/src/modules/workload/views/FutureInboundVolumeDetailView.vue frontend/src/modules/workload/utils/formatters.ts
git commit -m "feat: add carton and future inbound workload detail pages"
```

### Task 10: Align The Anomaly Dashboard And Anomaly Detail Pages With Warehouse Context

**Files:**
- Modify: `frontend/src/modules/dashboard/views/DashboardView.vue`
- Modify: `frontend/src/modules/anomalies/views/OutboundView.vue`
- Modify: `frontend/src/modules/anomalies/views/InboundView.vue`
- Modify: `frontend/src/modules/anomalies/views/ShelvingView.vue`
- Modify: `frontend/src/services/api.ts`
- Modify: `frontend/src/services/types.ts`

- [ ] **Step 1: Update the anomaly dashboard to read and switch `warehouseCode`**

```vue
<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const warehouseCode = ref(String(route.query.warehouseCode ?? 'DE'))

watch(warehouseCode, async (value) => {
  await router.replace({ query: { ...route.query, warehouseCode: value } })
  await load()
})
</script>
```

```vue
<el-radio-group v-model="warehouseCode">
  <el-radio-button label="DE">德国仓</el-radio-button>
  <el-radio-button label="ON">安大略仓</el-radio-button>
</el-radio-group>
```

- [ ] **Step 2: Update anomaly detail pages to bind route query into filters**

```ts
const route = useRoute()

const query = reactive<OutboundQuery>({
  warehouseCode: String(route.query.warehouseCode ?? 'DE'),
  riskStatus: String(route.query.riskStatus ?? ''),
  pageNo: 1,
  pageSize: 20,
  channel: '',
  customer: '',
  sortBy: 'deadlineAt',
  sortDirection: 'asc',
})
```

```vue
<el-select v-model="query.warehouseCode" placeholder="仓库" size="small" style="width:120px">
  <el-option label="德国仓" value="DE" />
  <el-option label="安大略仓" value="ON" />
</el-select>
```

- [ ] **Step 3: Keep overdue card jumps working without extra manual clicks**

```ts
onMounted(async () => {
  await fetchData()
})

watch(
  () => route.query,
  () => {
    query.warehouseCode = String(route.query.warehouseCode ?? 'DE')
    query.riskStatus = String(route.query.riskStatus ?? '')
    fetchData()
  },
)
```

- [ ] **Step 4: Run frontend tests and build**

Run: `npm --prefix frontend test -- --runInBand`

Expected: PASS.

Run: `npm --prefix frontend run build`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/modules/dashboard/views/DashboardView.vue frontend/src/modules/anomalies/views/OutboundView.vue frontend/src/modules/anomalies/views/InboundView.vue frontend/src/modules/anomalies/views/ShelvingView.vue frontend/src/services/api.ts frontend/src/services/types.ts
git commit -m "feat: sync anomaly pages with warehouse context"
```

### Task 11: Final Verification

**Files:**
- Verify only: `backend/Api.Tests/*`
- Verify only: `frontend/src/**/*`
- Verify only: `docs/superpowers/specs/2026-05-07-warehouse-workload-dashboard-design.md`

- [ ] **Step 1: Run the backend test suite**

Run: `dotnet test backend/Api.Tests/OpsMonitor.Api.Tests.csproj`

Expected: PASS.

- [ ] **Step 2: Run the frontend test suite**

Run: `npm --prefix frontend test -- --runInBand`

Expected: PASS.

- [ ] **Step 3: Run the frontend production build**

Run: `npm --prefix frontend run build`

Expected: PASS.

- [ ] **Step 4: Smoke-check the approved navigation paths manually**

Run these flows in the local dev app:

```text
1. 登录后默认进入 /dashboard
2. 切到 ON 仓，已超时待处理任务点击跳到 /anomalies/* 并自动带 warehouseCode=ON&riskStatus=overdue
3. 今天待出库包裹 -> /workloads/outbound-packages，默认时间范围为今天
4. 今天待到仓箱子 -> /workloads/cartons?context=today
5. 明天待到仓箱子 -> /workloads/cartons?context=tomorrow
6. 未来7天待到仓货量预估 -> /workloads/future-inbound-volume
7. /anomaly-dashboard 中切换仓库后摘要和异常列表仍能正常请求
```

Expected: All routes load, filters reflect the incoming query, and every detail page exports a CSV.

- [ ] **Step 5: Commit**

```bash
git add backend frontend
git commit -m "feat: deliver warehouse workload dashboard MVP"
```
