using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Pricing.Models;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Pricing;
using SalamHack.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Pricing.Queries.CalculatePricingQuote;

public sealed class CalculatePricingQuoteQueryHandler(
    IAppDbContext context,
    IServiceHistoryAnalyzer serviceHistoryAnalyzer)
    : IRequestHandler<CalculatePricingQuoteQuery, Result<PricingQuoteDto>>
{
    public async Task<Result<PricingQuoteDto>> Handle(CalculatePricingQuoteQuery query, CancellationToken ct)
    {
        var service = await context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.ServiceId && s.UserId == query.UserId, ct);

        if (service is null)
            return ApplicationErrors.Services.ServiceNotFound;

        if (!service.IsActive)
            return ApplicationErrors.Services.InactiveServiceCannotBeUsed;

        var history = await serviceHistoryAnalyzer.AnalyzeAsync(query.UserId, query.ServiceId, ct);
        var pricing = PricingCalculator.CalculateRecommendedPrices(
            query.EstimatedHours,
            query.Complexity,
            history.HoursOverrunFactor,
            history.CostOverrunFactor,
            history.AverageExtraExpenses);

        var naivePrice = Math.Round(
            query.EstimatedHours *
            service.DefaultHourlyRate *
            PricingCalculator.GetComplexityAdjustmentFactor(query.Complexity),
            2);

        var naiveMargin = naivePrice > 0
            ? Math.Round((naivePrice - pricing.RealCost) / naivePrice * 100, 1)
            : 0;

        var historyDto = new ServiceHistoryStatsDto(
            history.CompletedProjectCount,
            history.AverageEstimatedHours,
            history.AverageActualHours,
            history.AverageMarginPercent,
            history.HoursOverrunFactor,
            history.CostOverrunFactor,
            history.AverageExtraExpenses,
            history.HasHistory);

        var plans = new[]
        {
            new PricingPlanDto(
                "Economy",
                pricing.EconomyPrice,
                pricing.EconomyMarginPercent,
                CalculateAdvance(pricing.EconomyPrice),
                pricing.IsViableAtEconomy),
            new PricingPlanDto(
                "Recommended",
                pricing.MarketPrice,
                pricing.MarketMarginPercent,
                pricing.AdvanceAmount,
                true),
            new PricingPlanDto(
                "Premium",
                pricing.PremiumPrice,
                pricing.PremiumMarginPercent,
                CalculateAdvance(pricing.PremiumPrice),
                true)
        };

        return new PricingQuoteDto(
            service.Id,
            service.ServiceName,
            query.Complexity,
            query.EstimatedHours,
            pricing.AdjustedHours,
            pricing.RealCost,
            pricing.MinAcceptablePrice,
            pricing.TargetMarginPercent,
            naivePrice,
            naiveMargin,
            historyDto,
            plans,
            BuildInsights(history, naiveMargin));
    }

    private static decimal CalculateAdvance(decimal price)
        => Math.Round(price * ApplicationConstants.BusinessRules.AdvancePaymentRate, 2);

    private static IReadOnlyCollection<PricingInsightDto> BuildInsights(
        ServiceHistoryStats history,
        decimal naiveMargin)
    {
        if (!history.HasHistory)
        {
            return
            [
                new PricingInsightDto(
                    PricingInsightSeverity.Info,
                    "No completed project history exists for this service yet.")
            ];
        }

        var insights = new List<PricingInsightDto>
        {
            new(
                history.HoursOverrunFactor > 1.1m ? PricingInsightSeverity.Warning : PricingInsightSeverity.Success,
                $"Historical hours factor is {history.HoursOverrunFactor:0.##}x."),
            new(
                history.AverageExtraExpenses > 0 ? PricingInsightSeverity.Warning : PricingInsightSeverity.Success,
                $"Average extra expenses are {history.AverageExtraExpenses:0.##}."),
            new(
                history.AverageMarginPercent >= ApplicationConstants.BusinessRules.HealthyMarginThreshold
                    ? PricingInsightSeverity.Success
                    : PricingInsightSeverity.Warning,
                $"Historical average margin is {history.AverageMarginPercent:0.##}%.")
        };

        if (naiveMargin < ApplicationConstants.BusinessRules.AtRiskMarginThreshold)
        {
            insights.Add(new PricingInsightDto(
                PricingInsightSeverity.Critical,
                "Naive pricing would put this estimate below the at-risk margin threshold."));
        }

        return insights;
    }
}
