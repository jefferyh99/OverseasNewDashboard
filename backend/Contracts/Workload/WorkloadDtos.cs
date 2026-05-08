namespace OpsMonitor.Contracts.Workload;

public sealed record WarehouseOption(string Code, string Name);
public sealed record StatusSplit(int Total, int Processed, int Pending);
public sealed record MetricSplit(double Total, double Processed, double Pending);

public sealed record TodayInboundSummary(
    MetricSplit TotalCartons,
    MetricSplit TotalWeightKg,
    MetricSplit TotalVolumeM3,
    MetricSplit TotalUnits,
    MetricSplit TotalSkuCount,
    int SeaContainerCount,
    int TruckPalletCount);

public sealed record InboundTransportRow(
    string TransportMode,
    MetricSplit Cartons,
    MetricSplit WeightKg,
    MetricSplit VolumeM3,
    MetricSplit Units,
    MetricSplit SkuCount,
    int? SeaContainerCount,
    int? TruckPalletCount);

public sealed record TomorrowInboundSummary(
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int TotalUnits,
    int TotalSkuCount,
    int SeaContainerCount,
    int TruckPalletCount);

public sealed record TomorrowInboundTransportRow(
    string TransportMode,
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int TotalUnits,
    int TotalSkuCount,
    int? SeaContainerCount,
    int? TruckPalletCount);

public sealed record OverdueTaskCard(string Code, string Title, int Total);

public sealed record FutureInboundForecastBar(
    string ArrivalDate,
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int SeaContainerCount,
    int TruckPalletCount);

public sealed record WorkloadDashboardResponse(
    WarehouseOption Warehouse,
    IReadOnlyList<OverdueTaskCard> OverdueTasks,
    StatusSplit TodayOutboundPackages,
    StatusSplit TodayShelvingSkus,
    TodayInboundSummary TodayInboundSummary,
    IReadOnlyList<InboundTransportRow> TodayInbound,
    TomorrowInboundSummary TomorrowInboundSummary,
    IReadOnlyList<TomorrowInboundTransportRow> TomorrowInbound,
    IReadOnlyList<FutureInboundForecastBar> FutureInboundForecast);

public sealed record OutboundPackageDetailItem(
    string OrderId,
    string Customer,
    string LogisticsProvider,
    string TrackingNo,
    string ProductService,
    string ShippingRule,
    DateTimeOffset OrderTime,
    DateTimeOffset DeadlineAt,
    DateTimeOffset? ActualOutboundTime,
    string ProcessingStatus);

public sealed record OutboundPackageDetailResponse(
    StatusSplit Summary,
    IReadOnlyList<OutboundPackageDetailItem> Items);

public sealed record SkuDetailItem(
    string InventoryCode,
    string Sku,
    string Customer,
    string CartonId,
    string AsnId,
    string TransportMode,
    int TotalUnits,
    int ProcessedUnits,
    int PendingUnits,
    DateTimeOffset ArrivalTime,
    DateTimeOffset DeadlineAt,
    DateTimeOffset? ActualShelvedAt,
    string ProcessingStatus);

public sealed record SkuDetailResponse(
    StatusSplit Summary,
    IReadOnlyList<SkuDetailItem> Items);

public sealed record CartonDetailSummary(
    int Total,
    int Processed,
    int Pending,
    double TotalWeightKg,
    double TotalVolumeM3);

public sealed record CartonDetailItem(
    string CartonId,
    string AsnId,
    string TransportMode,
    DateTimeOffset EtaAt,
    DateTimeOffset? ArrivedAt,
    double TotalWeightKg,
    double TotalVolumeM3,
    int TotalUnits,
    int TotalSkuCount,
    string ProcessingStatus);

public sealed record CartonDetailResponse(
    CartonDetailSummary Summary,
    IReadOnlyList<CartonDetailItem> Items);

public sealed record FutureInboundVolumeSummary(
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int TotalUnits,
    int TotalSkuCount,
    int SeaContainerCount,
    int TruckPalletCount);

public sealed record FutureInboundVolumeItem(
    string ArrivalDate,
    string TransportMode,
    int TotalCartons,
    double TotalWeightKg,
    double TotalVolumeM3,
    int TotalUnits,
    int TotalSkuCount,
    int? TruckPalletCount,
    int? SeaContainerCount);

public sealed record FutureInboundVolumeResponse(
    FutureInboundVolumeSummary Summary,
    IReadOnlyList<FutureInboundVolumeItem> Items);
