namespace OpsMonitor.Contracts.Notifications;

public sealed record ReminderLogDto(
    int Id,
    string ObjectType,
    string ObjectId,
    string EventType,
    string Channel,
    DateTimeOffset SentAt,
    bool Success,
    string? ErrorMessage);

public sealed record TriggerTestResponse(string Message, int SentCount);
