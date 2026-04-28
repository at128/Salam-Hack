using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Pricing.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Pricing.Queries.CalculatePricingQuote;

public sealed class CalculatePricingQuoteQueryHandler(
    IAppDbContext context,
    IServiceHistoryAnalyzer serviceHistoryAnalyzer)
    : IRequestHandler<CalculatePricingQuoteQuery, Result<PricingQuoteDto>>
{
    public async Task<Result<PricingQuoteDto>> Handle(CalculatePricingQuoteQuery query, CancellationToken ct)
    {
        var calculation = await PricingQuoteBuilder.CalculateAsync(
            context,
            serviceHistoryAnalyzer,
            query.UserId,
            query.ServiceId,
            query.EstimatedHours,
            query.Complexity,
            query.RecentProjectCount,
            ct);

        return calculation.IsError
            ? calculation.Errors
            : calculation.Value.Quote;
    }
}
