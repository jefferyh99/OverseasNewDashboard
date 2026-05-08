using OpsMonitor.Contracts.Settings;

namespace OpsMonitor.Application.Settings;

public sealed class AlertsSettingsStore
{
    private readonly Dictionary<string, AlertsConfigResponse> _configs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DE"] = new(
            WarehouseId: "DE",
            TimeZoneId: "Europe/Berlin",
            WeekendDays: ["Saturday", "Sunday"],
            HolidayDates: ["2026-01-01", "2026-04-03", "2026-04-06"],
            LeadTimes:
            [
                new("outbound", 1),
                new("inbound", 12),
                new("shelving", 12),
            ],
            OutboundRule: new("16:00", "15:00", "18:00"),
            SeverityThresholds: [new("outbound", "overdue-count", 20)],
            Receivers: new(["u001", "u002"], ["g001"], ["ops@example.com"]),
            Channels: [new("wechat", true), new("email", true)],
            UpdatedAt: DateTimeOffset.Now.AddDays(-1),
            UpdatedBy: "admin"),
        ["ON"] = new(
            WarehouseId: "ON",
            TimeZoneId: "America/Toronto",
            WeekendDays: ["Saturday", "Sunday"],
            HolidayDates: ["2026-01-01", "2026-07-01", "2026-12-25"],
            LeadTimes:
            [
                new("outbound", 2),
                new("inbound", 12),
                new("shelving", 12),
            ],
            OutboundRule: new("17:00", "16:00", "19:00"),
            SeverityThresholds: [new("outbound", "overdue-count", 20)],
            Receivers: new(["u101"], ["g101"], ["on@example.com"]),
            Channels: [new("wechat", true), new("email", true)],
            UpdatedAt: DateTimeOffset.Now.AddDays(-1),
            UpdatedBy: "admin"),
    };

    public AlertsConfigResponse Get(string warehouseId)
        => _configs.TryGetValue(warehouseId, out var config) ? config : _configs["DE"];

    public AlertsConfigResponse Save(SaveAlertsConfigRequest request)
    {
        var saved = new AlertsConfigResponse(
            WarehouseId: request.WarehouseId,
            TimeZoneId: request.TimeZoneId,
            WeekendDays: request.WeekendDays,
            HolidayDates: request.HolidayDates,
            LeadTimes: request.LeadTimes,
            OutboundRule: request.OutboundRule,
            SeverityThresholds: request.SeverityThresholds,
            Receivers: request.Receivers,
            Channels: request.Channels,
            UpdatedAt: DateTimeOffset.Now,
            UpdatedBy: "admin");

        _configs[request.WarehouseId] = saved;
        return saved;
    }
}
