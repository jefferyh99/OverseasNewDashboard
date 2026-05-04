namespace OpsMonitor.Contracts.Auth;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string DisplayName, IReadOnlyList<string> Permissions);
