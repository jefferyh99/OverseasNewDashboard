using Microsoft.AspNetCore.Mvc.Testing;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Workload;
using System.Net;
using System.Net.Http.Json;

namespace OpsMonitor.Api.Tests;

public class WorkloadControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
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
        Assert.DoesNotContain(result.Data.Items, item => item.TransportMode == "sea" && item.TruckPalletCount.HasValue);
        Assert.DoesNotContain(result.Data.Items, item => item.TransportMode == "truck" && item.SeaContainerCount.HasValue);
    }
}
