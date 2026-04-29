using Microsoft.AspNetCore.Diagnostics;
using SalamHack.Api.Responses;

namespace SalamHack.Api.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse<object?>.Fail(
                "An unexpected error occurred.",
                [
                    new ApiErrorDto(
                        "Unexpected",
                        "An unexpected error occurred.",
                        "Unexpected")
                ],
                httpContext.TraceIdentifier),
            ct);

        return true;
    }
}
