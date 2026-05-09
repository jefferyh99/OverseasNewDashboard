using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Application.Outbound;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Anomalies;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/anomalies")]
[Authorize]
public class AnomaliesController(
    WarehouseOutboundRuleProvider ruleProvider,
    OutboundDeadlineCalculator deadlineCalculator) : ControllerBase
{
    // ── 出库异常 ──────────────────────────────────────────────────

    [HttpGet("outbound")]
    public IActionResult GetOutbound(
        [FromQuery] string warehouseCode = "DE",
        [FromQuery] string? riskStatus = null,
        [FromQuery] string? channel = null,
        [FromQuery] string? customer = null,
        [FromQuery] DateTimeOffset? orderTimeStart = null,
        [FromQuery] DateTimeOffset? orderTimeEnd = null,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "deadlineAt",
        [FromQuery] string sortDirection = "asc")
    {
        var now = DateTimeOffset.Now;
        var orderPrefix = warehouseCode == "ON" ? "ON" : "DE";
        var rule = ruleProvider.GetRule(warehouseCode);
        var seeds = new[]
        {
            new { OrderId = $"{orderPrefix}-SO20260504001", Customer = "客户A", LogisticsProvider = "DHL", TrackingNo = $"{orderPrefix}TRK0001", ProductService = "Express", ShippingRule = "Standard", OrderTime = now.AddHours(-30), CurrentStatus = "待出库", Shipped = false },
            new { OrderId = $"{orderPrefix}-SO20260504002", Customer = "客户B", LogisticsProvider = "UPS", TrackingNo = $"{orderPrefix}TRK0002", ProductService = "Economy", ShippingRule = "Priority", OrderTime = now.AddHours(-4), CurrentStatus = "待出库", Shipped = false },
            new { OrderId = $"{orderPrefix}-SO20260504003", Customer = "客户C", LogisticsProvider = "FedEx", TrackingNo = $"{orderPrefix}TRK0003", ProductService = "Express", ShippingRule = "Standard", OrderTime = now.AddHours(-2), CurrentStatus = "已出库", Shipped = true },
        };

        var items = seeds.Select(seed =>
        {
            var deadline = deadlineCalculator.Calculate(seed.OrderTime, rule);
            var warningReached = now >= deadline.WarningAtUtc;
            var overdueReached = now >= deadline.OverdueAtUtc;
            var remainingMinutes = (int)Math.Round((deadline.OverdueAtUtc - now).TotalMinutes);
            var overdueMinutes = (int)Math.Round((now - deadline.OverdueAtUtc).TotalMinutes);
            var computedRiskStatus = overdueReached ? "overdue" : warningReached ? "imminent" : "normal";

            return new OutboundItem(
                OrderId: seed.OrderId,
                Customer: seed.Customer,
                LogisticsProvider: seed.LogisticsProvider,
                TrackingNo: seed.TrackingNo,
                ProductService: seed.ProductService,
                ShippingRule: seed.ShippingRule,
                OrderTime: seed.OrderTime,
                DeadlineAt: deadline.OverdueAtUtc,
                TimeStatus: overdueReached ? "overdue" : "remaining",
                TimeValueMinutes: overdueReached ? Math.Abs(overdueMinutes) : remainingMinutes,
                TimeValueLabel: overdueReached
                    ? $"超时 {Math.Abs(overdueMinutes) / 60.0:F1} 小时"
                    : $"剩余 {remainingMinutes / 60.0:F1} 小时",
                CurrentStatus: seed.CurrentStatus,
                Shipped: seed.Shipped,
                RiskStatus: computedRiskStatus);
        })
        .Where(x => !x.Shipped)
        .Where(x => x.RiskStatus is "imminent" or "overdue")
        .ToList();

        if (!string.IsNullOrWhiteSpace(riskStatus))
        {
            items = items.Where(x => x.RiskStatus == riskStatus).ToList();
        }

        var imminentCount = items.Count(x => x.RiskStatus == "imminent");
        var overdueCount = items.Count(x => x.RiskStatus == "overdue");

        var data = new OutboundResponse(
            Summary: new(items.Count, imminentCount, overdueCount),
            FilterOptions: new(["渠道A", "渠道B", "渠道C"], ["客户A", "客户B", "客户C"]),
            List: new(pageNo, pageSize, items.Count, items));

        return Ok(ApiResponse<OutboundResponse>.Ok(data));
    }

    // ── 到仓不齐 ──────────────────────────────────────────────────

    [HttpGet("inbound")]
    public IActionResult GetInbound(
        [FromQuery] string warehouseCode = "DE",
        [FromQuery] string? riskStatus = null,
        [FromQuery] string? etaDateStart = null,
        [FromQuery] string? etaDateEnd = null,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "deadlineAt",
        [FromQuery] string sortDirection = "asc")
    {
        var now = DateTimeOffset.Now;
        var asnPrefix = warehouseCode == "ON" ? "ON" : "DE";
        var items = Enumerable.Range(1, 4).Select(i => new InboundItem(
            AsnId: $"{asnPrefix}-ASN2026050400{i}",
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
        [FromQuery] string warehouseCode = "DE",
        [FromQuery] string? riskStatus = null,
        [FromQuery] string? arrivalDateStart = null,
        [FromQuery] string? arrivalDateEnd = null,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "deadlineAt",
        [FromQuery] string sortDirection = "asc")
    {
        var now = DateTimeOffset.Now;
        var cartonPrefix = warehouseCode == "ON" ? "ON" : "DE";
        var items = Enumerable.Range(1, 5).Select(i => new ShelvingItem(
            CartonId: $"{cartonPrefix}-CTN2026050400{i}",
            AsnId: $"{cartonPrefix}-ASN202605030{i:D2}",
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
