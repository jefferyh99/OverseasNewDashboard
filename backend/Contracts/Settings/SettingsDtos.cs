namespace OpsMonitor.Contracts.Settings;

public sealed record LeadTimeConfig(string MonitorType, int LeadTimeHours);

public sealed record SeverityThreshold(string MonitorType, string MetricCode, int ThresholdValue);

public sealed record AlertReceivers(
    IReadOnlyList<string> UserIds,
    IReadOnlyList<string> GroupIds,
    IReadOnlyList<string> Emails);

public sealed record AlertChannelConfig(string ChannelCode, bool Enabled);

public sealed record OutboundRuleConfig(
    string CutoffTimeStandard,
    string CutoffTimeDaylight,
    string OverdueTime);

public sealed record AlertsConfigResponse(
    string WarehouseId,
    string TimeZoneId,
    IReadOnlyList<string> WeekendDays,
    IReadOnlyList<string> HolidayDates,
    IReadOnlyList<LeadTimeConfig> LeadTimes,
    OutboundRuleConfig OutboundRule,
    IReadOnlyList<SeverityThreshold> SeverityThresholds,
    AlertReceivers Receivers,
    IReadOnlyList<AlertChannelConfig> Channels,
    DateTimeOffset UpdatedAt,
    string UpdatedBy);

// PUT 请求体（省略只读字段）
public sealed record SaveAlertsConfigRequest(
    string WarehouseId,
    string TimeZoneId,
    IReadOnlyList<string> WeekendDays,
    IReadOnlyList<string> HolidayDates,
    IReadOnlyList<LeadTimeConfig> LeadTimes,
    OutboundRuleConfig OutboundRule,
    IReadOnlyList<SeverityThreshold> SeverityThresholds,
    AlertReceivers Receivers,
    IReadOnlyList<AlertChannelConfig> Channels);
