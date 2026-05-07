using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpsMonitor.Application.Notifications;
using OpsMonitor.Domain;
using OpsMonitor.Infrastructure.Persistence;

namespace OpsMonitor.Infrastructure.Notifications;

/// <summary>
/// 提醒扫描与派发服务。
/// 规则（来自 PRD §11.2）：
///   - 对象首次进入预警窗口时，发送一次即将超时提醒
///   - 对象首次进入超时状态时，发送一次已超时提醒
///   - 仍未完成的已超时对象，每 ResendIntervalHours 小时汇总重发一次
/// </summary>
public sealed class ReminderDispatchService(
    IAnomalyDataSource dataSource,
    IEnumerable<INotificationChannel> channels,
    OpsMonitorDbContext db,
    IConfiguration config,
    ILogger<ReminderDispatchService> logger)
{
    private readonly int _resendHours =
        config.GetValue<int>("Notifications:ResendIntervalHours", 4);

    public async Task ScanAndSendAsync(CancellationToken ct = default)
    {
        var risks = await dataSource.GetCurrentRisksAsync(ct);
        if (risks.Count == 0) return;

        var now = DateTimeOffset.Now;

        var recentLogs = await db.Set<ReminderLog>()
            .Where(r => r.SentAt > now.AddHours(-_resendHours * 2))
            .ToListAsync(ct);

        var toSend = new List<RiskObject>();

        foreach (var risk in risks)
        {
            var lastLog = recentLogs
                .Where(r => r.ObjectType == risk.ObjectType
                         && r.ObjectId   == risk.ObjectId
                         && r.EventType  == risk.RiskStatus
                         && r.Success)
                .OrderByDescending(r => r.SentAt)
                .FirstOrDefault();

            if (lastLog is null)
                toSend.Add(risk);
            else if (risk.RiskStatus == "overdue" && (now - lastLog.SentAt).TotalHours >= _resendHours)
                toSend.Add(risk);
        }

        if (toSend.Count == 0)
        {
            logger.LogDebug("[ReminderScan] 本轮无需发送提醒。");
            return;
        }

        logger.LogInformation("[ReminderScan] 本轮需发送提醒：{Count} 条", toSend.Count);

        var imminentCount = toSend.Count(r => r.RiskStatus == "imminent");
        var overdueCount  = toSend.Count(r => r.RiskStatus == "overdue");

        var msg = new NotificationMessage(
            Title: $"运营提醒 — 即将超时 {imminentCount} 条，已超时 {overdueCount} 条",
            Body: BuildTop3Body(toSend),
            WarehouseName: "德国仓",
            ImminentCount: imminentCount,
            OverdueCount: overdueCount);

        foreach (var channel in channels)
        {
            var success = await channel.SendAsync(msg, ct);
            db.Set<ReminderLog>().AddRange(toSend.Select(r => new ReminderLog
            {
                ObjectType   = r.ObjectType,
                ObjectId     = r.ObjectId,
                EventType    = r.RiskStatus,
                Channel      = channel.ChannelCode,
                SentAt       = now,
                Success      = success,
                ErrorMessage = success ? null : "channel returned failure",
            }));
        }

        await db.SaveChangesAsync(ct);
    }

    private static string BuildTop3Body(IList<RiskObject> items) =>
        string.Join(Environment.NewLine,
            items.OrderByDescending(r => r.RiskStatus == "overdue").Take(9)
                 .Select(r => $"- {r.Label}"));
}
