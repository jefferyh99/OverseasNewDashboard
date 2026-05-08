namespace OpsMonitor.Application.Outbound;

public sealed record WarehouseOutboundRuleConfig(
    string TimeZoneId,
    IReadOnlyList<string> WeekendDays,
    IReadOnlyList<DateOnly> HolidayDates,
    TimeOnly CutoffTimeStandard,
    TimeOnly CutoffTimeDaylight,
    TimeOnly OverdueTime,
    TimeSpan WarningLeadTime);

public sealed record OutboundDeadlineResult(
    DateTimeOffset WarningAtUtc,
    DateTimeOffset OverdueAtUtc,
    string TimeZoneId);
