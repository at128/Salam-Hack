using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Pricing.Models;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Pricing;
using SalamHack.Domain.Projects;
using SalamHack.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Pricing;

internal static class PricingQuoteBuilder
{
    public const int DefaultRecentProjectCount = 5;

    public static async Task<Result<PricingQuoteCalculation>> CalculateAsync(
        IAppDbContext context,
        IServiceHistoryAnalyzer serviceHistoryAnalyzer,
        Guid userId,
        Guid serviceId,
        decimal estimatedHours,
        ComplexityLevel complexity,
        int recentProjectCount,
        CancellationToken ct)
    {
        var service = await context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.UserId == userId, ct);

        if (service is null)
            return ApplicationErrors.Services.ServiceNotFound;

        if (!service.IsActive)
            return ApplicationErrors.Services.InactiveServiceCannotBeUsed;

        var history = await serviceHistoryAnalyzer.AnalyzeAsync(userId, serviceId, ct);
        var pricing = PricingCalculator.CalculateRecommendedPrices(
            estimatedHours,
            complexity,
            history.HoursOverrunFactor,
            history.CostOverrunFactor,
            history.AverageExtraExpenses);

        var recentProjects = await GetRecentProjectsAsync(context, userId, serviceId, recentProjectCount, ct);
        var quote = BuildQuote(service, history, pricing, recentProjects, estimatedHours, complexity);

        return new PricingQuoteCalculation(service, history, pricing, quote);
    }

    public static decimal CalculateAdvance(decimal price)
        => Math.Round(price * ApplicationConstants.BusinessRules.AdvancePaymentRate, 2);

    public static decimal GetAdvanceAmount(PricingResult pricing, PricingPlanType planType)
        => planType == PricingPlanType.Recommended
            ? pricing.AdvanceAmount
            : CalculateAdvance(pricing.GetPrice(planType));

    private static PricingQuoteDto BuildQuote(
        Service service,
        ServiceHistoryStats history,
        PricingResult pricing,
        IReadOnlyCollection<ServiceHistoryProjectDto> recentProjects,
        decimal estimatedHours,
        ComplexityLevel complexity)
    {
        var naivePrice = Math.Round(
            estimatedHours *
            service.DefaultHourlyRate *
            PricingCalculator.GetComplexityAdjustmentFactor(complexity),
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
                PricingPlanType.Economy,
                "Economy",
                pricing.EconomyPrice,
                pricing.EconomyMarginPercent,
                CalculateAdvance(pricing.EconomyPrice),
                pricing.IsViableAtEconomy),
            new PricingPlanDto(
                PricingPlanType.Recommended,
                "Recommended",
                pricing.MarketPrice,
                pricing.MarketMarginPercent,
                pricing.AdvanceAmount,
                true),
            new PricingPlanDto(
                PricingPlanType.Premium,
                "Premium",
                pricing.PremiumPrice,
                pricing.PremiumMarginPercent,
                CalculateAdvance(pricing.PremiumPrice),
                true)
        };

        return new PricingQuoteDto(
            service.Id,
            service.ServiceName,
            complexity,
            estimatedHours,
            pricing.AdjustedHours,
            pricing.RealCost,
            pricing.MinAcceptablePrice,
            pricing.TargetMarginPercent,
            naivePrice,
            naiveMargin,
            historyDto,
            recentProjects,
            plans,
            BuildInsights(history, naiveMargin));
    }

    private static async Task<IReadOnlyCollection<ServiceHistoryProjectDto>> GetRecentProjectsAsync(
        IAppDbContext context,
        Guid userId,
        Guid serviceId,
        int take,
        CancellationToken ct)
    {
        var projects = await context.Projects
            .AsNoTracking()
            .Include(p => p.Expenses)
            .Where(p => p.UserId == userId &&
                        p.ServiceId == serviceId &&
                        p.Status == ProjectStatus.Completed)
            .OrderByDescending(p => p.EndDate)
            .Take(Math.Clamp(take, 0, 20))
            .ToListAsync(ct);

        return projects.Select(p =>
        {
            var actualHours = p.ActualHours > 0 ? p.ActualHours : p.EstimatedHours;
            var extraExpenses = p.Expenses.Sum(e => e.Amount);
            var actualCost = Project.CalculateRealCost(actualHours, p.ToolCost) + extraExpenses;

            return new ServiceHistoryProjectDto(
                p.Id,
                p.ProjectName,
                p.EstimatedHours,
                actualHours,
                p.SuggestedPrice,
                actualCost,
                extraExpenses,
                Project.CalculateMarginPercent(p.SuggestedPrice, actualCost),
                p.EndDate);
        }).ToList();
    }

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

internal sealed record PricingQuoteCalculation(
    Service Service,
    ServiceHistoryStats History,
    PricingResult Pricing,
    PricingQuoteDto Quote);
