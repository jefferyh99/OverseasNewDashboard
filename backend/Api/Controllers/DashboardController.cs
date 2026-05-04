using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Contracts.Dashboard;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    [HttpGet("summary")]
    public IActionResult GetSummary()
    {
        // Scaffold placeholder — replace with real application service call.
        var summary = new DashboardSummaryResponse(
            TotalAlerts: 0,
            ActiveAlerts: 0,
            ResolvedToday: 0,
            AsOf: DateTimeOffset.UtcNow);

        return Ok(summary);
    }
}
