namespace OpsMonitor.Contracts;

/// <summary>统一 API 响应包装体。</summary>
public sealed record ApiResponse<T>
{
    public bool Success { get; init; }
    public string Code { get; init; } = "OK";
    public string Message { get; init; } = "success";
    public string? TraceId { get; init; }
    public T? Data { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string? traceId = null) =>
        new() { Success = true, Code = "OK", Message = "success", Data = data, TraceId = traceId };

    public static ApiResponse<T> Fail(string code, string message, string? traceId = null, IReadOnlyList<string>? errors = null) =>
        new() { Success = false, Code = code, Message = message, TraceId = traceId, Errors = errors };
}
