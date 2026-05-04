namespace OpsMonitor.Contracts.Dashboard;

public record DashboardSummaryResponse(
    int TotalAlerts,
    int ActiveAlerts,
    int ResolvedToday,
    DateTimeOffset AsOf);
