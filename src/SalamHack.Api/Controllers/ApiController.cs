using System.Security.Claims;
using Asp.Versioning;
using SalamHack.Api.Responses;
using SalamHack.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace SalamHack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class ApiController : ControllerBase
{
    protected bool TryGetUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out userId);
    }

    protected IActionResult OkResponse<T>(T? data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message, HttpContext.TraceIdentifier));

    protected IActionResult CreatedResponse<T>(
        string? actionName,
        object? routeValues,
        T data,
        string? message = null)
    {
        var response = ApiResponse<T>.Ok(
            data,
            message ?? "Created successfully.",
            HttpContext.TraceIdentifier);

        return string.IsNullOrWhiteSpace(actionName)
            ? StatusCode(StatusCodes.Status201Created, response)
            : CreatedAtAction(actionName, routeValues, response);
    }

    protected IActionResult DeletedResponse(string message = "Deleted successfully.")
        => Ok(ApiResponse<object?>.Ok(null, message, HttpContext.TraceIdentifier));

    protected IActionResult AcceptedResponse<T>(T data, string? message = null)
        => Accepted(ApiResponse<T>.Ok(data, message, HttpContext.TraceIdentifier));

    protected IActionResult UnauthorizedResponse()
        => StatusCode(
            StatusCodes.Status401Unauthorized,
            ApiResponse<object?>.Fail(
                "Unauthorized.",
                [new ApiErrorDto("Auth.Unauthorized", "User is not authenticated.", ErrorKind.Unauthorized.ToString())],
                HttpContext.TraceIdentifier));

    protected IActionResult Problem(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object?>.Fail(
                    "Unexpected error.",
                    [new ApiErrorDto("Unexpected", "Unexpected error.", ErrorKind.Unexpected.ToString())],
                    HttpContext.TraceIdentifier));
        }

        var statusCode = errors.All(e => e.Type == ErrorKind.Validation)
            ? StatusCodes.Status400BadRequest
            : GetStatusCode(errors[0]);

        return StatusCode(
            statusCode,
            ApiResponse<object?>.Fail(
                GetErrorMessage(errors),
                errors.Select(ToApiError).ToList(),
                HttpContext.TraceIdentifier));
    }

    private static int GetStatusCode(Error error)
        => error.Type switch
        {
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.Failure => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError,
        };

    private static string GetErrorMessage(IReadOnlyCollection<Error> errors)
        => errors.Count == 1
            ? errors.First().Description
            : "One or more errors occurred.";

    private static ApiErrorDto ToApiError(Error error)
        => new(error.Code, error.Description, error.Type.ToString());
}
