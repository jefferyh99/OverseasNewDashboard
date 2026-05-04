namespace OpsMonitor.Contracts.Dashboard;

// ── 通用子类型 ────────────────────────────────────────────────────

public sealed record WarehouseInfo(string WarehouseId, string WarehouseName);

public sealed record AlertChannelStatus(string ChannelCode, string ChannelName, string Status);

public sealed record AnomalyPreviewItem(
    string? OrderId,
    string? AsnId,
    string? CartonId,
    string? CustomerOrChannel,
    DateTimeOffset? OrderTime,
    DateTimeOffset? FirstArrivalTime,
    DateTimeOffset? ArrivalTime,
    DateTimeOffset DeadlineAt,
    string? CurrentStatus,
    string RiskStatus,
    string TimeStatus,
    int TimeValueMinutes,
    string TimeValueLabel,
    int? PlannedCartonCount,
    int? ArrivedCartonCount,
    int? MissingCartonCount,
    int? SkuCount,
    int? UnshelvedSkuCount);

public sealed record AnomalyPreviewGroup(
    int Total,
    int ImminentCount,
    int OverdueCount,
    IReadOnlyList<AnomalyPreviewItem> Items);

public sealed record Forecast7DaysItem(
    string Date,
    int CartonCount,
    double Weight,
    double Volume,
    bool IsPeakDay);

// ── 聚合响应 ──────────────────────────────────────────────────────

public sealed record BaseStatus(
    DateTimeOffset LastSyncTime,
    string SyncStatus,
    int TodayReminderCount,
    int TodayOverdueCount,
    IReadOnlyList<AlertChannelStatus> AlertChannels,
    bool DelayedDataFlag,
    string? DelayedReason);

public sealed record TodayOverview(
    int OutboundRiskCount,
    int InboundRiskCount,
    int ShelvingRiskCount,
    string TodayVolumePressureLevel,
    string OperationTip);

public sealed record AnomalyPreview(
    AnomalyPreviewGroup Outbound,
    AnomalyPreviewGroup Inbound,
    AnomalyPreviewGroup Shelving);

public sealed record DashboardResponse(
    WarehouseInfo Warehouse,
    BaseStatus BaseStatus,
    TodayOverview TodayOverview,
    AnomalyPreview AnomalyPreview,
    IReadOnlyList<Forecast7DaysItem> Forecast7Days);
