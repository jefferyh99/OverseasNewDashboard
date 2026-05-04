namespace OpsMonitor.Application.Notifications;

/// <summary>当前风险对象的轻量表示，供提醒扫描使用。</summary>
public sealed record RiskObject(
    string ObjectType,  // outbound | inbound | shelving
    string ObjectId,
    string RiskStatus,  // imminent | overdue
    string Label);      // 用于消息正文

/// <summary>提供当前风险对象列表的数据源接口（生产时替换为真实数据访问）。</summary>
public interface IAnomalyDataSource
{
    Task<IReadOnlyList<RiskObject>> GetCurrentRisksAsync(CancellationToken ct = default);
}
