using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Diagnostics;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
[Authorize]
public class DiagnosticsController : ControllerBase
{
    [HttpGet("overview")]
    public IActionResult GetOverview()
    {
        var data = new DiagnosticsOverviewResponse(
            Warehouse: new("WH-US-001", "德国"),
            Sync: new(
                LastSyncTime: DateTimeOffset.Now.AddMinutes(-30),
                SyncStatus: "success",
                DelayedDataFlag: false,
                DelayedReason: null),
            Alerts: new(
                Channels: [new("wechat", "healthy"), new("email", "healthy")],
                TodayReminderCount: 16),
            DataAnomalies: new(
                Count: 3,
                TopExamples: ["出库订单缺少下单时间", "入库单缺少计划箱数"]));

        return Ok(ApiResponse<DiagnosticsOverviewResponse>.Ok(data));
    }
}
