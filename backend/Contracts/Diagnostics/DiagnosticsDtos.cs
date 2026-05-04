namespace OpsMonitor.Contracts.Diagnostics;

public sealed record DiagnosticsWarehouse(string WarehouseId, string WarehouseName);

public sealed record DiagnosticsSync(
    DateTimeOffset LastSyncTime,
    string SyncStatus,
    bool DelayedDataFlag,
    string? DelayedReason);

public sealed record DiagnosticsAlertChannel(string ChannelCode, string Status);

public sealed record DiagnosticsAlerts(
    IReadOnlyList<DiagnosticsAlertChannel> Channels,
    int TodayReminderCount);

public sealed record DiagnosticsDataAnomalies(int Count, IReadOnlyList<string> TopExamples);

public sealed record DiagnosticsOverviewResponse(
    DiagnosticsWarehouse Warehouse,
    DiagnosticsSync Sync,
    DiagnosticsAlerts Alerts,
    DiagnosticsDataAnomalies DataAnomalies);
