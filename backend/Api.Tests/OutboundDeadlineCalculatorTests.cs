using OpsMonitor.Application.Outbound;
using OpsMonitor.Application.Settings;
using OpsMonitor.Contracts.Settings;

namespace OpsMonitor.Api.Tests;

public class OutboundDeadlineCalculatorTests
{
    [Fact]
    public void Provider_uses_outbound_lead_time_hours_as_warning_window()
    {
        var store = new AlertsSettingsStore();
        store.Save(new SaveAlertsConfigRequest(
            WarehouseId: "DE",
            TimeZoneId: "Europe/Berlin",
            WorkingHours: new("08:00", "17:00"),
            WeekendDays: ["Saturday", "Sunday"],
            HolidayDates: ["2026-01-01"],
            LeadTimes:
            [
                new("outbound", 2),
            ],
            OutboundRule: new("16:00", "15:00", "18:00"),
            ShelvingRule: new("17:00", 12, 3),
            InboundIncompleteRule: new("17:00", 12, 3),
            Receivers: new(["u001"], ["g001"], ["ops@example.com"]),
            Channels: [new("wechat", true), new("email", true)]));

        var provider = new WarehouseOutboundRuleProvider(store);

        var rule = provider.GetRule("DE");

        Assert.Equal(TimeSpan.FromHours(2), rule.WarningLeadTime);
    }

    [Fact]
    public void Order_at_summer_cutoff_uses_same_day_deadline()
    {
        var calculator = new OutboundDeadlineCalculator(new WarehouseBusinessCalendarService());
        var rule = CreateGermanRule(TimeSpan.FromHours(1));
        var orderCreatedAtUtc = new DateTimeOffset(2026, 6, 2, 13, 0, 0, TimeSpan.Zero);

        var result = calculator.Calculate(orderCreatedAtUtc, rule);

        Assert.Equal(new DateTimeOffset(2026, 6, 2, 15, 0, 0, TimeSpan.Zero), result.WarningAtUtc);
        Assert.Equal(new DateTimeOffset(2026, 6, 2, 16, 0, 0, TimeSpan.Zero), result.OverdueAtUtc);
    }

    [Fact]
    public void Friday_after_cutoff_skips_to_monday()
    {
        var calculator = new OutboundDeadlineCalculator(new WarehouseBusinessCalendarService());
        var rule = CreateGermanRule(TimeSpan.FromHours(1));
        var orderCreatedAtUtc = new DateTimeOffset(2026, 6, 5, 14, 30, 0, TimeSpan.Zero);

        var result = calculator.Calculate(orderCreatedAtUtc, rule);

        Assert.Equal(new DateTimeOffset(2026, 6, 8, 15, 0, 0, TimeSpan.Zero), result.WarningAtUtc);
        Assert.Equal(new DateTimeOffset(2026, 6, 8, 16, 0, 0, TimeSpan.Zero), result.OverdueAtUtc);
    }

    [Fact]
    public void Holiday_order_skips_to_next_workday()
    {
        var calculator = new OutboundDeadlineCalculator(new WarehouseBusinessCalendarService());
        var rule = CreateGermanRule(TimeSpan.FromHours(1));
        var orderCreatedAtUtc = new DateTimeOffset(2026, 4, 3, 10, 0, 0, TimeSpan.Zero);

        var result = calculator.Calculate(orderCreatedAtUtc, rule);

        Assert.Equal(new DateTimeOffset(2026, 4, 7, 15, 0, 0, TimeSpan.Zero), result.WarningAtUtc);
        Assert.Equal(new DateTimeOffset(2026, 4, 7, 16, 0, 0, TimeSpan.Zero), result.OverdueAtUtc);
    }

    [Fact]
    public void Winter_workday_uses_standard_cutoff()
    {
        var calculator = new OutboundDeadlineCalculator(new WarehouseBusinessCalendarService());
        var rule = CreateGermanRule(TimeSpan.FromHours(1));
        var orderCreatedAtUtc = new DateTimeOffset(2026, 1, 6, 14, 30, 0, TimeSpan.Zero);

        var result = calculator.Calculate(orderCreatedAtUtc, rule);

        Assert.Equal(new DateTimeOffset(2026, 1, 6, 16, 0, 0, TimeSpan.Zero), result.WarningAtUtc);
        Assert.Equal(new DateTimeOffset(2026, 1, 6, 17, 0, 0, TimeSpan.Zero), result.OverdueAtUtc);
    }

    private static WarehouseOutboundRuleConfig CreateGermanRule(TimeSpan warningLeadTime) => new(
        TimeZoneId: "Europe/Berlin",
        WeekendDays: ["Saturday", "Sunday"],
        HolidayDates: [new DateOnly(2026, 1, 1), new DateOnly(2026, 4, 3), new DateOnly(2026, 4, 6)],
        CutoffTimeStandard: new TimeOnly(16, 0),
        CutoffTimeDaylight: new TimeOnly(15, 0),
        OverdueTime: new TimeOnly(18, 0),
        WarningLeadTime: warningLeadTime);
}
