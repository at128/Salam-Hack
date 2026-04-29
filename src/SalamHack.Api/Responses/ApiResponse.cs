namespace SalamHack.Api.Responses;

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message,
    IReadOnlyCollection<ApiErrorDto> Errors,
    string TraceId)
{
    public static ApiResponse<T> Ok(T? data, string? message, string traceId)
        => new(true, data, message, [], traceId);

    public static ApiResponse<T> Fail(
        string message,
        IReadOnlyCollection<ApiErrorDto> errors,
        string traceId)
        => new(false, default, message, errors, traceId);
}

public sealed record ApiErrorDto(
    string Code,
    string Message,
    string Type);
