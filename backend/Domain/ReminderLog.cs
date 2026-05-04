namespace OpsMonitor.Domain;

/// <summary>提醒发送记录，用于防重发和审计。</summary>
public class ReminderLog
{
    public int Id { get; set; }

    /// <summary>监控类型：outbound | inbound | shelving</summary>
    public string ObjectType { get; set; } = string.Empty;

    /// <summary>业务对象 ID（订单号 / ASN 号 / 箱号）</summary>
    public string ObjectId { get; set; } = string.Empty;

    /// <summary>事件类型：imminent | overdue</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>推送渠道：wechat | email</summary>
    public string Channel { get; set; } = string.Empty;

    public DateTimeOffset SentAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
