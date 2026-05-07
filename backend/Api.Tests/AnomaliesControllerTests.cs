using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace OpsMonitor.Api.Tests;

public class AnomaliesControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Outbound_with_warehouseCode_and_riskStatus_returns_ok()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        var response = await client.GetAsync("/api/anomalies/outbound?warehouseCode=DE&riskStatus=overdue");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
