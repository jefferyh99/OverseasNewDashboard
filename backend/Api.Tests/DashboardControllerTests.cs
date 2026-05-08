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
    public async Task Dashboard_with_warehouseCode_ON_returns_ON_warehouse_id()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        var response = await client.GetAsync("/api/dashboard?warehouseCode=ON");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardResponse>>();
        Assert.Equal("ON", result!.Data!.Warehouse.WarehouseId);
    }

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
