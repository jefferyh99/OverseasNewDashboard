using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Anomalies;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/anomalies")]
[Authorize]
public class AnomaliesController : ControllerBase
{
    // ── 出库异常 ──────────────────────────────────────────────────

    [HttpGet("outbound")]
    public IActionResult GetOutbound(
        [FromQuery] string? riskStatus,
        [FromQuery] string? channel,
        [FromQuery] string? customer,
        [FromQuery] DateTimeOffset? orderTimeStart,
        [FromQuery] DateTimeOffset? orderTimeEnd,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "deadlineAt",
        [FromQuery] string sortDirection = "asc")
    {
        var now = DateTimeOffset.Now;
        var items = Enumerable.Range(1, 5).Select(i => new OutboundItem(
            OrderId: $"SO2026050400{i}",
            CustomerOrChannel: i % 2 == 0 ? "客户A / 渠道A" : "客户B / 渠道B",
            OrderTime: now.AddHours(-i * 6),
            DeadlineAt: now.AddHours(i % 3 == 0 ? -2 : 2),
            TimeStatus: i % 3 == 0 ? "overdue" : "remaining",
            TimeValueMinutes: i % 3 == 0 ? 120 : 180,
            TimeValueLabel: i % 3 == 0 ? $"超时 2 小时" : $"剩余 {3 * i} 小时",
            CurrentStatus: "待出库",
            Shipped: false,
            RiskStatus: i % 3 == 0 ? "overdue" : "imminent")).ToList();

        var data = new OutboundResponse(
            Summary: new(128, 48, 80),
            FilterOptions: new(["渠道A", "渠道B"], ["客户A", "客户B"]),
            List: new(pageNo, pageSize, 128, items));

        return Ok(ApiResponse<OutboundResponse>.Ok(data));
    }

    // ── 到仓不齐 ──────────────────────────────────────────────────

    [HttpGet("inbound")]
    public IActionResult GetInbound(
        [FromQuery] string? riskStatus,
        [FromQuery] string? etaDateStart,
        [FromQuery] string? etaDateEnd,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "deadlineAt",
        [FromQuery] string sortDirection = "asc")
    {
        var now = DateTimeOffset.Now;
        var items = Enumerable.Range(1, 4).Select(i => new InboundItem(
            AsnId: $"ASN2026050400{i}",
            EtaDate: now.AddDays(-i).ToString("yyyy-MM-dd"),
            PlannedCartonCount: 50,
            ArrivedCartonCount: 50 - i * 2,
            MissingCartonCount: i * 2,
            FirstArrivalTime: now.AddDays(-i),
            DeadlineAt: now.AddHours(i % 2 == 0 ? -3 : 6),
            TimeStatus: i % 2 == 0 ? "overdue" : "remaining",
            TimeValueMinutes: i % 2 == 0 ? 180 : 360,
            TimeValueLabel: i % 2 == 0 ? "超时 3 小时" : "剩余 6 小时",
            RiskStatus: i % 2 == 0 ? "overdue" : "imminent")).ToList();

        var data = new InboundResponse(
            Summary: new(32, 11, 21),
            List: new(pageNo, pageSize, 32, items));

        return Ok(ApiResponse<InboundResponse>.Ok(data));
    }

    // ── 上架异常 ──────────────────────────────────────────────────

    [HttpGet("shelving")]
    public IActionResult GetShelving(
        [FromQuery] string? riskStatus,
        [FromQuery] string? arrivalDateStart,
        [FromQuery] string? arrivalDateEnd,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "deadlineAt",
        [FromQuery] string sortDirection = "asc")
    {
        var now = DateTimeOffset.Now;
        var items = Enumerable.Range(1, 5).Select(i => new ShelvingItem(
            CartonId: $"CTN2026050400{i}",
            AsnId: $"ASN202605030{i:D2}",
            ArrivalTime: now.AddDays(-i),
            SkuCount: 18,
            UnshelvedSkuCount: i * 2,
            DeadlineAt: now.AddHours(i % 3 == 0 ? -1 : 3),
            TimeStatus: i % 3 == 0 ? "overdue" : "remaining",
            TimeValueMinutes: i % 3 == 0 ? 90 : 180,
            TimeValueLabel: i % 3 == 0 ? "超时 1.5 小时" : "剩余 3 小时",
            RiskStatus: i % 3 == 0 ? "overdue" : "imminent")).ToList();

        var data = new ShelvingResponse(
            Summary: new(54, 18, 36),
            List: new(pageNo, pageSize, 54, items));

        return Ok(ApiResponse<ShelvingResponse>.Ok(data));
    }
}
