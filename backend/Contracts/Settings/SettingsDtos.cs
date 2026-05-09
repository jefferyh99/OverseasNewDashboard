namespace OpsMonitor.Contracts.Settings;

public sealed record LeadTimeConfig(string MonitorType, int LeadTimeHours);

public sealed record AlertReceivers(
    IReadOnlyList<string> UserIds,
    IReadOnlyList<string> GroupIds,
    IReadOnlyList<string> Emails);

public sealed record AlertChannelConfig(string ChannelCode, bool Enabled);

public sealed record OutboundRuleConfig(
    string CutoffTimeStandard,
    string CutoffTimeDaylight,
    string OverdueTime);

public sealed record WarehouseWorkingHoursConfig(
    string StartTime,
    string EndTime);

public sealed record TimelinessRuleConfig(
    string OverdueTime,
    int WarningLeadHours,
    int SlaDays);

public sealed record AlertsConfigResponse(
    string WarehouseId,
    string TimeZoneId,
    WarehouseWorkingHoursConfig WorkingHours,
    IReadOnlyList<string> WeekendDays,
    IReadOnlyList<string> HolidayDates,
    IReadOnlyList<LeadTimeConfig> LeadTimes,
    OutboundRuleConfig OutboundRule,
    TimelinessRuleConfig ShelvingRule,
    TimelinessRuleConfig InboundIncompleteRule,
    AlertReceivers Receivers,
    IReadOnlyList<AlertChannelConfig> Channels,
    DateTimeOffset UpdatedAt,
    string UpdatedBy);

// PUT 请求体（省略只读字段）
public sealed record SaveAlertsConfigRequest(
    string WarehouseId,
    string TimeZoneId,
    WarehouseWorkingHoursConfig WorkingHours,
    IReadOnlyList<string> WeekendDays,
    IReadOnlyList<string> HolidayDates,
    IReadOnlyList<LeadTimeConfig> LeadTimes,
    OutboundRuleConfig OutboundRule,
    TimelinessRuleConfig ShelvingRule,
    TimelinessRuleConfig InboundIncompleteRule,
    AlertReceivers Receivers,
    IReadOnlyList<AlertChannelConfig> Channels);
