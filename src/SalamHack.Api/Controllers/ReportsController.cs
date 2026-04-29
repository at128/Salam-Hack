using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Reports.Commands.ExportProfitabilityReport;
using SalamHack.Application.Features.Reports.Queries.GetCashFlowForecast;
using SalamHack.Application.Features.Reports.Queries.GetProfitabilityReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class ReportsController(ISender sender) : ApiController
{
    [HttpGet("profitability")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetProfitability(
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetProfitabilityReportQuery(userId, fromUtc, toUtc), ct);

        return result.Match(report => OkResponse(report), Problem);
    }

    [HttpGet("cash-flow")]
    [HttpGet("cashflow")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetCashFlow(
        [FromQuery] Guid? delayedCustomerId,
        [FromQuery] DateTimeOffset? asOfUtc,
        [FromQuery] decimal openingBalance = 0,
        [FromQuery] DateTimeOffset? openingBalanceDateUtc = null,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetCashFlowForecastQuery(userId, delayedCustomerId, asOfUtc, openingBalance, openingBalanceDateUtc),
            ct);

        return result.Match(forecast => OkResponse(forecast), Problem);
    }

    [HttpPost("profitability/export")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> ExportProfitability(
        [FromBody] ExportProfitabilityRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new ExportProfitabilityReportCommand(userId, request.Format, request.FromUtc, request.ToUtc),
            ct);

        return result.Match(export => OkResponse(export, "Report exported successfully."), Problem);
    }
}

public sealed record ExportProfitabilityRequest(
    ReportExportFormat Format,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc);
