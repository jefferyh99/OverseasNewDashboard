namespace OpsMonitor.Application.Notifications;

/// <summary>
/// Mock 数据源 — 在业务系统只读库接入前提供测试数据。
/// 生产时注册真实实现替换此类。
/// </summary>
public sealed class MockAnomalyDataSource : IAnomalyDataSource
{
    public Task<IReadOnlyList<RiskObject>> GetCurrentRisksAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.Now;
        var list = new List<RiskObject>
        {
            new("outbound", "SO20260504001", "overdue",  "出库 SO20260504001 已超时 2 小时"),
            new("outbound", "SO20260504002", "imminent", "出库 SO20260504002 剩余 3 小时"),
            new("outbound", "SO20260504003", "overdue",  "出库 SO20260504003 已超时 5 小时"),
            new("inbound",  "ASN20260504001","imminent", "到仓不齐 ASN20260504001 剩余 6 小时"),
            new("inbound",  "ASN20260504002","overdue",  "到仓不齐 ASN20260504002 已超时 3 小时"),
            new("shelving", "CTN20260504001","overdue",  "上架 CTN20260504001 已超时 1.5 小时"),
            new("shelving", "CTN20260504002","imminent", "上架 CTN20260504002 剩余 2 小时"),
        };

        return Task.FromResult<IReadOnlyList<RiskObject>>(list);
    }
}
