using SalamHack.Application.Features.Invoices.Queries.GetPayments;
using SalamHack.Application.Features.Invoices.Queries.GetPaymentsSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class PaymentsController(ISender sender) : ApiController
{
    [HttpGet]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetPayments(
        [FromQuery] Guid? invoiceId,
        [FromQuery] DateTimeOffset? fromDate,
        [FromQuery] DateTimeOffset? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetPaymentsQuery(userId, invoiceId, fromDate, toDate, pageNumber, pageSize),
            ct);

        return result.Match(payments => OkResponse(payments), Problem);
    }

    [HttpGet("summary")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTimeOffset? asOfUtc,
        [FromQuery] int overdueInvoiceLimit = 10,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetPaymentsSummaryQuery(userId, asOfUtc, overdueInvoiceLimit),
            ct);

        return result.Match(summary => OkResponse(summary), Problem);
    }
}
