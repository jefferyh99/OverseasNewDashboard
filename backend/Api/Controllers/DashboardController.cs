using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Dashboard;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var now = DateTimeOffset.Now;

        var data = new DashboardResponse(
            Warehouse: new("WH-US-001", "美国 1 号仓"),
            BaseStatus: new(
                LastSyncTime: now.AddMinutes(-30),
                SyncStatus: "success",
                TodayReminderCount: 16,
                TodayOverdueCount: 9,
                AlertChannels:
                [
                    new("wechat", "企业微信", "healthy"),
                    new("email", "邮件", "healthy"),
                ],
                DelayedDataFlag: false,
                DelayedReason: null),
            TodayOverview: new(
                OutboundRiskCount: 23,
                InboundRiskCount: 7,
                ShelvingRiskCount: 15,
                TodayVolumePressureLevel: "medium",
                OperationTip: "今天主要风险集中在出库时效，建议优先处理渠道 A 的待出库订单。"),
            AnomalyPreview: new(
                Outbound: new(23, 10, 13, []),
                Inbound: new(7, 2, 5, []),
                Shelving: new(15, 4, 11, [])),
            Forecast7Days: Enumerable.Range(1, 7).Select(i => new Forecast7DaysItem(
                Date: now.AddDays(i).ToString("yyyy-MM-dd"),
                CartonCount: 100 + i * 8,
                Weight: 1200 + i * 50,
                Volume: 28 + i * 0.8,
                IsPeakDay: i % 4 == 0)).ToList());

        return Ok(ApiResponse<DashboardResponse>.Ok(data));
    }
}
