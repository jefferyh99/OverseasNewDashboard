using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsMonitor.Application.Outbound;
using OpsMonitor.Contracts;
using OpsMonitor.Contracts.Dashboard;

namespace OpsMonitor.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(
    WarehouseOutboundRuleProvider ruleProvider,
    OutboundDeadlineCalculator deadlineCalculator) : ControllerBase
{
    private static readonly TimeSpan DelayThreshold = TimeSpan.FromMinutes(90);

    [HttpGet]
    public IActionResult Get([FromQuery] string warehouseCode = "DE")
    {
        var now = DateTimeOffset.Now;
        var lastSync = now.AddMinutes(-30);
        var delayedDataFlag = (now - lastSync) > DelayThreshold;

        var warehouse = warehouseCode == "ON"
            ? new WarehouseInfo("ON", "Ontario")
            : new WarehouseInfo("DE", "Germany");

        var orderPrefix = warehouseCode == "ON" ? "ON" : "DE";
        var rule = ruleProvider.GetRule(warehouseCode);

        var outboundItems = new[]
        {
            new { OrderId = $"{orderPrefix}-SO20260504001", Customer = "CustomerA", LogisticsProvider = "DHL", TrackingNo = $"{orderPrefix}TRK0001", ProductService = "Express", ShippingRule = "Standard", OrderTime = now.AddHours(-30), CurrentStatus = "pending-outbound", Shipped = false },
            new { OrderId = $"{orderPrefix}-SO20260504002", Customer = "CustomerB", LogisticsProvider = "UPS", TrackingNo = $"{orderPrefix}TRK0002", ProductService = "Economy", ShippingRule = "Priority", OrderTime = now.AddHours(-4), CurrentStatus = "pending-outbound", Shipped = false },
            new { OrderId = $"{orderPrefix}-SO20260504003", Customer = "CustomerC", LogisticsProvider = "FedEx", TrackingNo = $"{orderPrefix}TRK0003", ProductService = "Express", ShippingRule = "Standard", OrderTime = now.AddHours(-2), CurrentStatus = "shipped", Shipped = true },
        }
        .Select(seed =>
        {
            var deadline = deadlineCalculator.Calculate(seed.OrderTime, rule);
            var warningReached = now >= deadline.WarningAtUtc;
            var overdueReached = now >= deadline.OverdueAtUtc;
            var remainingMinutes = (int)Math.Round((deadline.OverdueAtUtc - now).TotalMinutes);
            var overdueMinutes = (int)Math.Round((now - deadline.OverdueAtUtc).TotalMinutes);
            var riskStatus = overdueReached ? "overdue" : warningReached ? "imminent" : "normal";

            return new
            {
                seed.OrderId,
                seed.Customer,
                seed.LogisticsProvider,
                seed.TrackingNo,
                seed.ProductService,
                seed.ShippingRule,
                seed.OrderTime,
                seed.CurrentStatus,
                seed.Shipped,
                DeadlineAt = deadline.OverdueAtUtc,
                TimeStatus = overdueReached ? "overdue" : "remaining",
                TimeValueMinutes = overdueReached ? Math.Abs(overdueMinutes) : remainingMinutes,
                TimeValueLabel = overdueReached
                    ? $"超时 {Math.Abs(overdueMinutes) / 60.0:F1} 小时"
                    : $"剩余 {remainingMinutes / 60.0:F1} 小时",
                RiskStatus = riskStatus,
            };
        })
        .Where(x => !x.Shipped)
        .Where(x => x.RiskStatus is "imminent" or "overdue")
        .Select(x => new AnomalyPreviewItem(
            OrderId: x.OrderId,
            AsnId: null,
            CartonId: null,
            Customer: x.Customer,
            LogisticsProvider: x.LogisticsProvider,
            TrackingNo: x.TrackingNo,
            ProductService: x.ProductService,
            ShippingRule: x.ShippingRule,
            OrderTime: x.OrderTime,
            FirstArrivalTime: null,
            ArrivalTime: null,
            DeadlineAt: x.DeadlineAt,
            CurrentStatus: x.CurrentStatus,
            RiskStatus: x.RiskStatus,
            TimeStatus: x.TimeStatus,
            TimeValueMinutes: x.TimeValueMinutes,
            TimeValueLabel: x.TimeValueLabel,
            PlannedCartonCount: null,
            ArrivedCartonCount: null,
            MissingCartonCount: null,
            SkuCount: null,
            UnshelvedSkuCount: null))
        .ToList();

        var inboundItems = Enumerable.Range(1, 5).Select(i =>
        {
            var isOverdue = i % 2 == 0;
            var minutesDiff = isOverdue ? -(i * 60) : (i * 120);
            return new AnomalyPreviewItem(
                OrderId: null,
                AsnId: $"ASN2026050400{i}",
                CartonId: null,
                Customer: null,
                LogisticsProvider: null,
                TrackingNo: null,
                ProductService: null,
                ShippingRule: null,
                OrderTime: null,
                FirstArrivalTime: now.AddDays(-i),
                ArrivalTime: null,
                DeadlineAt: now.AddMinutes(minutesDiff),
                CurrentStatus: null,
                RiskStatus: isOverdue ? "overdue" : "imminent",
                TimeStatus: isOverdue ? "overdue" : "remaining",
                TimeValueMinutes: Math.Abs(minutesDiff),
                TimeValueLabel: isOverdue
                    ? $"超时 {Math.Abs(minutesDiff) / 60} 小时"
                    : $"剩余 {minutesDiff / 60} 小时",
                PlannedCartonCount: 50,
                ArrivedCartonCount: 50 - i * 2,
                MissingCartonCount: i * 2,
                SkuCount: null,
                UnshelvedSkuCount: null);
        }).ToList();

        var shelvingItems = Enumerable.Range(1, 5).Select(i =>
        {
            var isOverdue = i % 3 == 0;
            var minutesDiff = isOverdue ? -(i * 30) : (i * 90);
            return new AnomalyPreviewItem(
                OrderId: null,
                AsnId: $"ASN202605030{i:D2}",
                CartonId: $"CTN2026050400{i}",
                Customer: null,
                LogisticsProvider: null,
                TrackingNo: null,
                ProductService: null,
                ShippingRule: null,
                OrderTime: null,
                FirstArrivalTime: null,
                ArrivalTime: now.AddDays(-i),
                DeadlineAt: now.AddMinutes(minutesDiff),
                CurrentStatus: null,
                RiskStatus: isOverdue ? "overdue" : "imminent",
                TimeStatus: isOverdue ? "overdue" : "remaining",
                TimeValueMinutes: Math.Abs(minutesDiff),
                TimeValueLabel: isOverdue
                    ? $"超时 {Math.Abs(minutesDiff / 60.0):F1} 小时"
                    : $"剩余 {minutesDiff / 60} 小时",
                PlannedCartonCount: null,
                ArrivedCartonCount: null,
                MissingCartonCount: null,
                SkuCount: 18,
                UnshelvedSkuCount: i * 2);
        }).ToList();

        var forecast = Enumerable.Range(1, 7).Select(i => new
        {
            Date = now.AddDays(i).ToString("yyyy-MM-dd"),
            CartonCount = 100 + i * 8,
            Weight = 1200 + i * 50.0,
            Volume = 28 + i * 0.8,
        }).ToList();

        var avgCartons = forecast.Average(f => f.CartonCount);
        var forecast7Days = forecast.Select(f => new Forecast7DaysItem(
            Date: f.Date,
            CartonCount: f.CartonCount,
            Weight: f.Weight,
            Volume: f.Volume,
            IsPeakDay: f.CartonCount > avgCartons * 1.3)).ToList();

        var data = new DashboardResponse(
            Warehouse: warehouse,
            BaseStatus: new(
                LastSyncTime: lastSync,
                SyncStatus: delayedDataFlag ? "delayed" : "success",
                TodayReminderCount: 16,
                TodayOverdueCount: outboundItems.Count(x => x.RiskStatus == "overdue")
                    + inboundItems.Count(x => x.RiskStatus == "overdue")
                    + shelvingItems.Count(x => x.RiskStatus == "overdue"),
                AlertChannels:
                [
                    new("wechat", "WeCom", "healthy"),
                    new("email", "Email", "healthy"),
                ],
                DelayedDataFlag: delayedDataFlag,
                DelayedReason: delayedDataFlag ? "Business system sync delayed" : null),
            TodayOverview: new(
                OutboundRiskCount: outboundItems.Count,
                InboundRiskCount: inboundItems.Count,
                ShelvingRiskCount: shelvingItems.Count,
                TodayVolumePressureLevel: "medium",
                OperationTip: "Prioritize outbound tasks first."),
            AnomalyPreview: new(
                Outbound: new(
                    outboundItems.Count,
                    outboundItems.Count(x => x.RiskStatus == "imminent"),
                    outboundItems.Count(x => x.RiskStatus == "overdue"),
                    outboundItems),
                Inbound: new(
                    inboundItems.Count,
                    inboundItems.Count(x => x.RiskStatus == "imminent"),
                    inboundItems.Count(x => x.RiskStatus == "overdue"),
                    inboundItems),
                Shelving: new(
                    shelvingItems.Count,
                    shelvingItems.Count(x => x.RiskStatus == "imminent"),
                    shelvingItems.Count(x => x.RiskStatus == "overdue"),
                    shelvingItems)),
            Forecast7Days: forecast7Days);

        return Ok(ApiResponse<DashboardResponse>.Ok(data));
    }
}
