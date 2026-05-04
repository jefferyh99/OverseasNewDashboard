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
    // 数据延迟阈值（超过此时长认为数据可能延迟）
    private static readonly TimeSpan DelayThreshold = TimeSpan.FromMinutes(90);

    [HttpGet]
    public IActionResult Get()
    {
        var now = DateTimeOffset.Now;
        var lastSync = now.AddMinutes(-30); // mock：30 分钟前同步
        var delayedDataFlag = (now - lastSync) > DelayThreshold;

        // ── 出库预览前5条 ────────────────────────────────────────
        var outboundItems = Enumerable.Range(1, 5).Select(i =>
        {
            var isOverdue = i % 3 == 0;
            var minutesDiff = isOverdue ? -(i * 40) : (i * 55);
            return new AnomalyPreviewItem(
                OrderId: $"SO2026050400{i}",
                AsnId: null, CartonId: null,
                CustomerOrChannel: i % 2 == 0 ? "客户A / 渠道A" : "客户B / 渠道B",
                OrderTime: now.AddHours(-i * 8),
                FirstArrivalTime: null, ArrivalTime: null,
                DeadlineAt: now.AddMinutes(minutesDiff),
                CurrentStatus: "待出库",
                RiskStatus: isOverdue ? "overdue" : "imminent",
                TimeStatus: isOverdue ? "overdue" : "remaining",
                TimeValueMinutes: Math.Abs(minutesDiff),
                TimeValueLabel: isOverdue
                    ? $"超时 {Math.Abs(minutesDiff) / 60} 小时"
                    : $"剩余 {minutesDiff / 60} 小时",
                PlannedCartonCount: null, ArrivedCartonCount: null,
                MissingCartonCount: null, SkuCount: null, UnshelvedSkuCount: null);
        }).ToList();

        // ── 到仓不齐预览前5条 ─────────────────────────────────────
        var inboundItems = Enumerable.Range(1, 5).Select(i =>
        {
            var isOverdue = i % 2 == 0;
            var minutesDiff = isOverdue ? -(i * 60) : (i * 120);
            return new AnomalyPreviewItem(
                OrderId: null, CartonId: null,
                AsnId: $"ASN2026050400{i}",
                CustomerOrChannel: null,
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
                SkuCount: null, UnshelvedSkuCount: null);
        }).ToList();

        // ── 上架预览前5条 ─────────────────────────────────────────
        var shelvingItems = Enumerable.Range(1, 5).Select(i =>
        {
            var isOverdue = i % 3 == 0;
            var minutesDiff = isOverdue ? -(i * 30) : (i * 90);
            return new AnomalyPreviewItem(
                OrderId: null,
                AsnId: $"ASN202605030{i:D2}",
                CartonId: $"CTN2026050400{i}",
                CustomerOrChannel: null,
                OrderTime: null, FirstArrivalTime: null,
                ArrivalTime: now.AddDays(-i),
                DeadlineAt: now.AddMinutes(minutesDiff),
                CurrentStatus: null,
                RiskStatus: isOverdue ? "overdue" : "imminent",
                TimeStatus: isOverdue ? "overdue" : "remaining",
                TimeValueMinutes: Math.Abs(minutesDiff),
                TimeValueLabel: isOverdue
                    ? $"超时 {Math.Abs(minutesDiff / 60.0):F1} 小时"
                    : $"剩余 {minutesDiff / 60} 小时",
                PlannedCartonCount: null, ArrivedCartonCount: null, MissingCartonCount: null,
                SkuCount: 18, UnshelvedSkuCount: i * 2);
        }).ToList();

        // ── 7 天货量预测（含高峰日正确识别）────────────────────────
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
            Warehouse: new("WH-US-001", "美国 1 号仓"),
            BaseStatus: new(
                LastSyncTime: lastSync,
                SyncStatus: delayedDataFlag ? "delayed" : "success",
                TodayReminderCount: 16,
                TodayOverdueCount: 9,
                AlertChannels:
                [
                    new("wechat", "企业微信", "healthy"),
                    new("email", "邮件", "healthy"),
                ],
                DelayedDataFlag: delayedDataFlag,
                DelayedReason: delayedDataFlag ? "业务系统同步超时" : null),
            TodayOverview: new(
                OutboundRiskCount: 23,
                InboundRiskCount: 7,
                ShelvingRiskCount: 15,
                TodayVolumePressureLevel: "medium",
                OperationTip: "今天主要风险集中在出库时效，建议优先处理渠道 A 的待出库订单。"),
            AnomalyPreview: new(
                Outbound: new(23, 10, 13, outboundItems),
                Inbound: new(7, 2, 5, inboundItems),
                Shelving: new(15, 4, 11, shelvingItems)),
            Forecast7Days: forecast7Days);

        return Ok(ApiResponse<DashboardResponse>.Ok(data));
    }
}
