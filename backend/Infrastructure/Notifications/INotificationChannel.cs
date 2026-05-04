namespace OpsMonitor.Infrastructure.Notifications;

/// <summary>通知消息结构。</summary>
public sealed record NotificationMessage(
    string Title,
    string Body,
    string WarehouseName,
    int ImminentCount,
    int OverdueCount);

/// <summary>通知渠道抽象。</summary>
public interface INotificationChannel
{
    string ChannelCode { get; }
    Task<bool> SendAsync(NotificationMessage message, CancellationToken ct = default);
}
