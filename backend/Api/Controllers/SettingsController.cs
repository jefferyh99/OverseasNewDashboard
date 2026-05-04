using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Settings;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    // 模拟内存存储（真实实现替换为数据库）
    private static AlertsConfigResponse _config = new(
        WarehouseId: "WH-US-001",
        LeadTimes:
        [
            new("outbound", 4),
            new("inbound", 12),
            new("shelving", 12),
        ],
        SeverityThresholds: [new("outbound", "overdue-count", 20)],
        Receivers: new(["u001", "u002"], ["g001"], ["ops@example.com"]),
        Channels: [new("wechat", true), new("email", true)],
        UpdatedAt: DateTimeOffset.Now.AddDays(-1),
        UpdatedBy: "admin");

    [HttpGet("alerts")]
    public IActionResult GetAlerts() =>
        Ok(ApiResponse<AlertsConfigResponse>.Ok(_config));

    [HttpPut("alerts")]
    public IActionResult SaveAlerts([FromBody] SaveAlertsConfigRequest request)
    {
        _config = new(
            WarehouseId: request.WarehouseId,
            LeadTimes: request.LeadTimes,
            SeverityThresholds: request.SeverityThresholds,
            Receivers: request.Receivers,
            Channels: request.Channels,
            UpdatedAt: DateTimeOffset.Now,
            UpdatedBy: "admin");

        return Ok(ApiResponse<AlertsConfigResponse>.Ok(_config));
    }
}
