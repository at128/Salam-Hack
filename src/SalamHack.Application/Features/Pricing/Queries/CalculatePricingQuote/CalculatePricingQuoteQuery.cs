using SalamHack.Application.Features.Pricing.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;
using MediatR;

namespace SalamHack.Application.Features.Pricing.Queries.CalculatePricingQuote;

public sealed record CalculatePricingQuoteQuery(
    Guid UserId,
    Guid ServiceId,
    decimal EstimatedHours,
    ComplexityLevel Complexity,
    int RecentProjectCount = PricingQuoteBuilder.DefaultRecentProjectCount) : IRequest<Result<PricingQuoteDto>>;
