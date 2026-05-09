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
        Assert.Equal("08:00", payload.Data.WorkingHours.StartTime);
        Assert.Equal("17:00", payload.Data.WorkingHours.EndTime);
        Assert.Equal("16:00", payload.Data.OutboundRule.CutoffTimeStandard);
        Assert.Equal("15:00", payload.Data.OutboundRule.CutoffTimeDaylight);
        Assert.Equal("18:00", payload.Data.OutboundRule.OverdueTime);
        Assert.Equal(1, payload.Data.LeadTimes.Single(x => x.MonitorType == "outbound").LeadTimeHours);
        Assert.Equal(3, payload.Data.ShelvingRule.SlaDays);
        Assert.Equal(3, payload.Data.InboundIncompleteRule.SlaDays);
        Assert.Contains("Saturday", payload.Data.WeekendDays);
        Assert.NotEmpty(payload.Data.HolidayDates);
    }

    [Fact]
    public async Task Alerts_put_then_get_persists_outbound_rule_fields_for_warehouse()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        const string warehouseId = "DE-TEMP";
        var request = new SaveAlertsConfigRequest(
            WarehouseId: warehouseId,
            TimeZoneId: "Europe/Berlin",
            WorkingHours: new("08:30", "17:30"),
            WeekendDays: ["Saturday", "Sunday"],
            HolidayDates: ["2026-12-24"],
            LeadTimes:
            [
                new("outbound", 2),
            ],
            OutboundRule: new("17:00", "16:00", "19:00"),
            ShelvingRule: new("18:00", 10, 4),
            InboundIncompleteRule: new("18:30", 11, 5),
            Receivers: new(["u010"], ["g010"], ["de-ops@example.com"]),
            Channels: [new("wechat", true), new("email", false)]);

        var saveResponse = await client.PutAsJsonAsync("/api/settings/alerts", request);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/settings/alerts?warehouseId={warehouseId}");
        var payload = await getResponse.Content.ReadFromJsonAsync<ApiResponse<AlertsConfigResponse>>();

        Assert.NotNull(payload?.Data);
        Assert.Equal("17:00", payload!.Data!.OutboundRule.CutoffTimeStandard);
        Assert.Equal("19:00", payload.Data.OutboundRule.OverdueTime);
        Assert.Equal(2, payload.Data.LeadTimes.Single(x => x.MonitorType == "outbound").LeadTimeHours);
        Assert.Equal("08:30", payload.Data.WorkingHours.StartTime);
        Assert.Equal(10, payload.Data.ShelvingRule.WarningLeadHours);
        Assert.Equal(5, payload.Data.InboundIncompleteRule.SlaDays);
        Assert.Equal(["2026-12-24"], payload.Data.HolidayDates);
    }

    [Fact]
    public async Task Alerts_all_returns_all_configured_warehouses()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        var response = await client.GetAsync("/api/settings/alerts/all");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<AlertsConfigResponse>>>();
        Assert.NotNull(payload?.Data);
        Assert.Contains(payload!.Data!, x => x.WarehouseId == "DE");
        Assert.Contains(payload.Data!, x => x.WarehouseId == "ON");
    }
}
