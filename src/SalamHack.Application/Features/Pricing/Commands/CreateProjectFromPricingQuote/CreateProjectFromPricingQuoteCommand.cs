using SalamHack.Application.Features.Pricing.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Pricing;
using SalamHack.Domain.Projects;
using MediatR;

namespace SalamHack.Application.Features.Pricing.Commands.CreateProjectFromPricingQuote;

public sealed record CreateProjectFromPricingQuoteCommand(
    Guid UserId,
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
    DateTimeOffset EndDate) : IRequest<Result<ProjectFromPricingQuoteDto>>;
