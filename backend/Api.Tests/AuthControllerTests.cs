using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Auth;

namespace OpsMonitor.Api.Tests;

public class AuthControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Login_with_valid_credentials_returns_mock_token()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin", "admin"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        Assert.Equal("mock-jwt-token", result!.Data!.Token);
        Assert.Equal("系统管理员", result.Data.DisplayName);
        Assert.Equal("admin", result.Data.UserId);
    }

    [Fact]
    public async Task Login_with_invalid_credentials_returns_401()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("wrong", "credentials"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_requires_auth()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_returns_200_with_mock_token()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");
        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Auth_validate_returns_valid_with_mock_token()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");
        var response = await client.GetAsync("/api/auth/validate");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ValidateResponse>>();
        Assert.True(result!.Data!.Valid);
    }

    [Fact]
    public async Task Auth_permissions_returns_workload_and_anomaly_menu_permissions()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");
        var response = await client.GetAsync("/api/auth/permissions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PermissionsResponse>>();
        var permissions = result!.Data!.MenuPermissions;

        Assert.Contains("dashboard", permissions);
        Assert.Contains("workload-dashboard", permissions);
        Assert.Contains("anomaly-dashboard", permissions);
        Assert.Contains("workload-outbound-detail", permissions);
        Assert.Contains("workload-sku-detail", permissions);
        Assert.Contains("workload-carton-detail", permissions);
        Assert.Contains("workload-future-inbound-volume", permissions);
        Assert.Contains("anomaly-outbound", permissions);
        Assert.Contains("anomaly-inbound", permissions);
        Assert.Contains("anomaly-shelving", permissions);
        Assert.Contains("settings-alerts", permissions);
    }
}
