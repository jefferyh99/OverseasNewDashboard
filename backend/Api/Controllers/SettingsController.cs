using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Application.Settings;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Settings;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController(AlertsSettingsStore settingsStore) : ControllerBase
{
    [HttpGet("alerts")]
    public IActionResult GetAlerts([FromQuery] string warehouseId = "DE") =>
        Ok(ApiResponse<AlertsConfigResponse>.Ok(settingsStore.Get(warehouseId)));

    [HttpPut("alerts")]
    public IActionResult SaveAlerts([FromBody] SaveAlertsConfigRequest request) =>
        Ok(ApiResponse<AlertsConfigResponse>.Ok(settingsStore.Save(request)));
}
