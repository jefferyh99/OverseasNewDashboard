using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Contracts.Auth;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Mock: accept admin/admin only. Replace with real auth adapter.
        if (request.Username != "admin" || request.Password != "admin")
            return Unauthorized(new { message = "Invalid credentials." });

        var response = new LoginResponse(
            Token: "mock-jwt-token",
            DisplayName: "Administrator",
            Permissions: ["dashboard:view", "anomaly:view", "settings:view"]);

        return Ok(response);
    }
}
