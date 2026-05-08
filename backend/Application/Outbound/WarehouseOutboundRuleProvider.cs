using OpsMonitor.Application.Settings;

namespace OpsMonitor.Application.Outbound;

public sealed class WarehouseOutboundRuleProvider(AlertsSettingsStore settingsStore)
{
    public WarehouseOutboundRuleConfig GetRule(string warehouseId)
    {
        var config = settingsStore.Get(warehouseId);
        var outboundLeadTime = config.LeadTimes.Single(x => x.MonitorType == "outbound");

        return new WarehouseOutboundRuleConfig(
            TimeZoneId: config.TimeZoneId,
            WeekendDays: config.WeekendDays,
            HolidayDates: config.HolidayDates.Select(DateOnly.Parse).ToArray(),
            CutoffTimeStandard: TimeOnly.Parse(config.OutboundRule.CutoffTimeStandard),
            CutoffTimeDaylight: TimeOnly.Parse(config.OutboundRule.CutoffTimeDaylight),
            OverdueTime: TimeOnly.Parse(config.OutboundRule.OverdueTime),
            WarningLeadTime: TimeSpan.FromHours(outboundLeadTime.LeadTimeHours));
    }
}
