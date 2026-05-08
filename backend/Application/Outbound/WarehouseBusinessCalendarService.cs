namespace OpsMonitor.Application.Outbound;

public sealed class WarehouseBusinessCalendarService
{
    public bool IsWorkingDay(DateOnly date, WarehouseOutboundRuleConfig rule)
    {
        var day = date.DayOfWeek.ToString();
        return !rule.WeekendDays.Contains(day) && !rule.HolidayDates.Contains(date);
    }

    public DateOnly GetNextWorkingDay(DateOnly date, WarehouseOutboundRuleConfig rule)
    {
        var current = date;
        do
        {
            current = current.AddDays(1);
        } while (!IsWorkingDay(current, rule));

        return current;
    }
}
