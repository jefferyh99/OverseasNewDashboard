namespace OpsMonitor.Contracts.Anomalies;

// ── 通用分页 ──────────────────────────────────────────────────────

public sealed record PagedList<T>(int PageNo, int PageSize, int Total, IReadOnlyList<T> Items);

public sealed record AnomalySummary(int Total, int ImminentCount, int OverdueCount);

// ── 出库异常 ──────────────────────────────────────────────────────

public sealed record OutboundItem(
    string OrderId,
    string Customer,
    string LogisticsProvider,
    string TrackingNo,
    string ProductService,
    string ShippingRule,
    DateTimeOffset OrderTime,
    DateTimeOffset DeadlineAt,
    string TimeStatus,
    int TimeValueMinutes,
    string TimeValueLabel,
    string CurrentStatus,
    bool Shipped,
    string RiskStatus);

public sealed record OutboundFilterOptions(
    IReadOnlyList<string> Channels,
    IReadOnlyList<string> Customers);

public sealed record OutboundResponse(
    AnomalySummary Summary,
    OutboundFilterOptions FilterOptions,
    PagedList<OutboundItem> List);

// ── 到仓不齐 ──────────────────────────────────────────────────────

public sealed record InboundItem(
    string AsnId,
    string EtaDate,
    int PlannedCartonCount,
    int ArrivedCartonCount,
    int MissingCartonCount,
    DateTimeOffset FirstArrivalTime,
    DateTimeOffset DeadlineAt,
    string TimeStatus,
    int TimeValueMinutes,
    string TimeValueLabel,
    string RiskStatus);

public sealed record InboundResponse(
    AnomalySummary Summary,
    PagedList<InboundItem> List);

// ── 上架异常 ──────────────────────────────────────────────────────

public sealed record ShelvingItem(
    string CartonId,
    string AsnId,
    DateTimeOffset ArrivalTime,
    int SkuCount,
    int UnshelvedSkuCount,
    DateTimeOffset DeadlineAt,
    string TimeStatus,
    int TimeValueMinutes,
    string TimeValueLabel,
    string RiskStatus);

public sealed record ShelvingResponse(
    AnomalySummary Summary,
    PagedList<ShelvingItem> List);
