namespace OpsMonitor.Application.Outbound;

public sealed class OutboundDeadlineCalculator(WarehouseBusinessCalendarService calendar)
{
    public OutboundDeadlineResult Calculate(DateTimeOffset orderCreatedAtUtc, WarehouseOutboundRuleConfig rule)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(rule.TimeZoneId);
        var localOrder = TimeZoneInfo.ConvertTime(orderCreatedAtUtc, timeZone);
        var localDate = DateOnly.FromDateTime(localOrder.DateTime);
        var localTime = TimeOnly.FromDateTime(localOrder.DateTime);

        DateOnly promiseDate;
        if (!calendar.IsWorkingDay(localDate, rule))
        {
            promiseDate = calendar.GetNextWorkingDay(localDate, rule);
        }
        else
        {
            var cutoff = timeZone.IsDaylightSavingTime(localOrder.DateTime)
                ? rule.CutoffTimeDaylight
                : rule.CutoffTimeStandard;

            promiseDate = localTime <= cutoff
                ? localDate
                : calendar.GetNextWorkingDay(localDate, rule);
        }

        var overdueLocal = promiseDate.ToDateTime(rule.OverdueTime, DateTimeKind.Unspecified);
        var overdueUtc = new DateTimeOffset(overdueLocal, timeZone.GetUtcOffset(overdueLocal)).ToUniversalTime();
        var warningUtc = overdueUtc - rule.WarningLeadTime;

        return new OutboundDeadlineResult(warningUtc, overdueUtc, rule.TimeZoneId);
    }
}
