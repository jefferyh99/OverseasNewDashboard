using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Auth;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    [HttpOptions("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Username != "admin" || request.Password != "admin")
            return Unauthorized(ApiResponse<object>.Fail("UNAUTHORIZED", "用户名或密码错误"));

        var data = new LoginResponse(
            Token: "mock-jwt-token",
            TokenType: "Bearer",
            ExpiresIn: 7200,
            UserId: "admin",
            UserName: "admin",
            DisplayName: "系统管理员");

        return Ok(ApiResponse<LoginResponse>.Ok(data));
    }

    [HttpGet("validate")]
    [Authorize]
    public IActionResult Validate()
    {
        var data = new ValidateResponse(
            Valid: true,
            UserId: "admin",
            UserName: "admin",
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(2));

        return Ok(ApiResponse<ValidateResponse>.Ok(data));
    }

    [HttpGet("permissions")]
    [Authorize]
    public IActionResult GetPermissions()
    {
        var data = new PermissionsResponse(
            MenuPermissions:
            [
                "dashboard",
                "workload-dashboard",
                "anomaly-dashboard",
                "workload-outbound-detail",
                "workload-sku-detail",
                "workload-carton-detail",
                "workload-future-inbound-volume",
                "anomaly-outbound",
                "anomaly-inbound",
                "anomaly-shelving",
                "settings-alerts",
                "prototype-wecom-notification"
            ],
            ButtonPermissions: ["settings-alerts-save"]);

        return Ok(ApiResponse<PermissionsResponse>.Ok(data));
    }
}
