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
