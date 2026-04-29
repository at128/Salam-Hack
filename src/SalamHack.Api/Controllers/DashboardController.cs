using SalamHack.Application.Features.Dashboard.Queries.GetDashboardSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class DashboardController(ISender sender) : ApiController
{
    [HttpGet("summary")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTimeOffset? asOfUtc,
        [FromQuery] int recentTransactionCount = 6,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetDashboardSummaryQuery(userId, asOfUtc, recentTransactionCount),
            ct);

        return result.Match(summary => OkResponse(summary), Problem);
    }
}
