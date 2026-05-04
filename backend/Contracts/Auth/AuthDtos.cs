namespace OpsMonitor.Contracts.Auth;

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(
    string Token,
    string TokenType,
    int ExpiresIn,
    string UserId,
    string UserName,
    string DisplayName);

public sealed record ValidateResponse(
    bool Valid,
    string UserId,
    string UserName,
    DateTimeOffset ExpiresAt);

public sealed record PermissionsResponse(
    IReadOnlyList<string> MenuPermissions,
    IReadOnlyList<string> ButtonPermissions);
