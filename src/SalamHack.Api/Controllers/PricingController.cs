using SalamHack.Application.Features.Pricing.Commands.CreateInvoiceFromPricingQuote;
using SalamHack.Application.Features.Pricing.Commands.CreateProjectFromPricingQuote;
using SalamHack.Application.Features.Pricing.Queries.CalculatePricingQuote;
using SalamHack.Domain.Pricing;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class PricingController(ISender sender) : ApiController
{
    [HttpGet("quote")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> CalculateQuote(
        [FromQuery] Guid serviceId,
        [FromQuery] decimal estimatedHours,
        [FromQuery] ComplexityLevel complexity,
        [FromQuery] int recentProjectCount = 5,
        [FromQuery] decimal toolCost = 0,
        [FromQuery(Name = "revision")] int? requestedRevisions = null,
        [FromQuery] bool isUrgent = false,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new CalculatePricingQuoteQuery(
                userId,
                serviceId,
                estimatedHours,
                complexity,
                recentProjectCount,
                toolCost,
                requestedRevisions,
                isUrgent),
            ct);

        return result.Match(quote => OkResponse(quote), Problem);
    }

    [HttpPost("projects")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateProjectFromQuote(
        [FromBody] CreateProjectFromQuoteRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CreateProjectFromPricingQuoteCommand(
            userId,
            request.CustomerId,
            request.ServiceId,
            request.ProjectName,
            request.EstimatedHours,
            request.Complexity,
            request.SelectedPlan,
            request.ToolCost,
            request.Revision,
            request.IsUrgent,
            request.StartDate,
            request.EndDate), ct);

        return result.Match(
            created => CreatedResponse(null, null, created, "Project created from quote successfully."),
            Problem);
    }

    [HttpPost("invoices")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateInvoiceFromQuote(
        [FromBody] CreateInvoiceFromQuoteRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CreateInvoiceFromPricingQuoteCommand(
            userId,
            request.CustomerId,
            request.ServiceId,
            request.ProjectName,
            request.EstimatedHours,
            request.Complexity,
            request.SelectedPlan,
            request.ToolCost,
            request.Revision,
            request.IsUrgent,
            request.StartDate,
            request.EndDate,
            request.InvoiceNumber,
            request.IssueDate,
            request.DueDate,
            request.Currency,
            request.Notes), ct);

        return result.Match(
            created => CreatedResponse(null, null, created, "Invoice created from quote successfully."),
            Problem);
    }
}

public sealed record CreateProjectFromQuoteRequest(
    Guid CustomerId,
    Guid ServiceId,
    string ProjectName,
    decimal EstimatedHours,
    ComplexityLevel Complexity,
    PricingPlanType SelectedPlan,
    decimal ToolCost,
    int Revision,
    bool IsUrgent,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);

public sealed record CreateInvoiceFromQuoteRequest(
    Guid CustomerId,
    Guid ServiceId,
    string ProjectName,
    decimal EstimatedHours,
    ComplexityLevel Complexity,
    PricingPlanType SelectedPlan,
    decimal ToolCost,
    int Revision,
    bool IsUrgent,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string InvoiceNumber,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    string Currency,
    string? Notes);
