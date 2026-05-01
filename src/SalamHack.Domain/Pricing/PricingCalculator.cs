using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Projects;

namespace SalamHack.Domain.Pricing;

public static class PricingCalculator
{
    /// <summary>
    /// Returns the recommended pricing tiers and cost summary for a basic estimate.
    /// </summary>
    public static PricingResult CalculateRecommendedPrices(
        decimal estimatedHours,
        ComplexityLevel complexity)
        => CalculateRecommendedPrices(estimatedHours, complexity, 1.0m, 1.0m, 0m);

    /// <summary>
    /// Returns the recommended pricing tiers and cost summary after applying historical overrun factors.
    /// </summary>
    public static PricingResult CalculateRecommendedPrices(
        decimal estimatedHours,
        ComplexityLevel complexity,
        decimal hoursOverrunFactor,
        decimal costOverrunFactor,
        decimal avgExtraExpenses,
        decimal toolCost = 0,
        decimal hourlyRate = ApplicationConstants.BusinessRules.DefaultRevenueRatePerHour,
        int includedRevisions = 0,
        int requestedRevisions = 0,
        bool isUrgent = false,
        bool hasReliableHistory = true)
    {
        var complexityMultiplier = GetComplexityAdjustmentFactor(complexity);
        var safeHoursFactor = NormalizeFactor(
            hoursOverrunFactor,
            ApplicationConstants.BusinessRules.MinimumHistoricalHoursFactor,
            ApplicationConstants.BusinessRules.MaximumHistoricalHoursFactor);
        var appliedCostFactor = GetResidualCostFactor(costOverrunFactor, safeHoursFactor);
        var safeHourlyRate = hourlyRate > 0
            ? hourlyRate
            : ApplicationConstants.BusinessRules.DefaultRevenueRatePerHour;

        var safeIncludedRevisions = Math.Max(0, includedRevisions);
        var safeRequestedRevisions = Math.Max(0, requestedRevisions);
        var extraRevisionCount = Math.Max(0, safeRequestedRevisions - safeIncludedRevisions);

        var adjustedHours = Math.Round(estimatedHours * safeHoursFactor * complexityMultiplier, 1);
        var costRate = safeHourlyRate * ApplicationConstants.BusinessRules.CostToRevenueRatio;
        var laborCost = adjustedHours * costRate * appliedCostFactor;
        var realCost = Math.Round(laborCost + avgExtraExpenses + toolCost, 2);

        var costBasedPrice = Math.Round(realCost / (1 - ApplicationConstants.BusinessRules.TargetProfitMarginRate), 2);
        var hourlyFloorPrice = Math.Round(adjustedHours * safeHourlyRate, 2);
        var minAcceptable = Math.Round(realCost * ApplicationConstants.BusinessRules.MinimumPriceMultiplier, 2);
        var baseRecommendedPrice = Math.Max(Math.Max(costBasedPrice, hourlyFloorPrice), minAcceptable);

        var revisionMultiplier = 1 + extraRevisionCount * ApplicationConstants.BusinessRules.ExtraRevisionPriceRate;
        var urgencyMultiplier = isUrgent
            ? ApplicationConstants.BusinessRules.UrgentProjectPriceMultiplier
            : 1m;
        var confidenceMultiplier = hasReliableHistory
            ? 1m
            : 1 + ApplicationConstants.BusinessRules.NewServiceConfidenceBufferRate;

        var marketPrice = Math.Round(
            baseRecommendedPrice * revisionMultiplier * urgencyMultiplier * confidenceMultiplier,
            2);
        var economyPrice = Math.Round(
            Math.Max(
                marketPrice * ApplicationConstants.BusinessRules.EconomyPriceMultiplier,
                minAcceptable),
            2);
        var premiumPrice = Math.Round(marketPrice * ApplicationConstants.BusinessRules.PremiumPriceMultiplier, 2);
        var advanceAmount = Math.Round(marketPrice * ApplicationConstants.BusinessRules.AdvancePaymentRate, 2);

        return new PricingResult(
            EconomyPrice: economyPrice,
            MarketPrice: marketPrice,
            PremiumPrice: premiumPrice,
            MinAcceptablePrice: minAcceptable,
            AdvanceAmount: advanceAmount,
            RealCost: realCost,
            AdjustedHours: adjustedHours,
            TargetMarginPercent: ApplicationConstants.BusinessRules.TargetProfitMarginRate * 100,
            ComplexityMultiplier: complexityMultiplier,
            HistoricalHoursFactor: safeHoursFactor,
            AppliedCostFactor: appliedCostFactor,
            HourlyFloorPrice: hourlyFloorPrice,
            CostBasedPrice: costBasedPrice,
            BaseRecommendedPrice: baseRecommendedPrice,
            IncludedRevisions: safeIncludedRevisions,
            RequestedRevisions: safeRequestedRevisions,
            ExtraRevisionCount: extraRevisionCount,
            IsUrgent: isUrgent,
            RevisionMultiplier: revisionMultiplier,
            UrgencyMultiplier: urgencyMultiplier,
            ConfidenceMultiplier: confidenceMultiplier);
    }

    /// <summary>
    /// Returns the projected absolute profit after changing the current price by a percentage.
    /// </summary>
    public static decimal CalculateProfitAfterPriceChange(decimal currentPrice, decimal totalCost, decimal changePercent)
    {
        var newPrice = currentPrice * (1 + changePercent / 100);
        return Math.Round(newPrice - totalCost, 2);
    }

    /// <summary>
    /// Returns the projected absolute profit after reducing the current total cost by a percentage.
    /// </summary>
    public static decimal CalculateProfitAfterCostReduction(decimal price, decimal totalCost, decimal reductionPercent)
    {
        var newCost = totalCost * (1 - reductionPercent / 100);
        return Math.Round(price - newCost, 2);
    }

    /// <summary>
    /// Returns the projected profit per hour after adding extra actual hours.
    /// </summary>
    public static decimal CalculateHourlyProfitAfterExtraHours(decimal profit, decimal currentHours, decimal extraHours)
    {
        var totalHours = currentHours + extraHours;
        return totalHours > 0 ? Math.Round(profit / totalHours, 2) : 0;
    }

    /// <summary>
    /// Returns the multiplier applied to estimated hours for the selected complexity level.
    /// </summary>
    public static decimal GetComplexityAdjustmentFactor(ComplexityLevel complexity)
        => complexity switch
        {
            ComplexityLevel.Simple => ApplicationConstants.BusinessRules.SimpleComplexityMultiplier,
            ComplexityLevel.Medium => ApplicationConstants.BusinessRules.MediumComplexityMultiplier,
            ComplexityLevel.Complex => ApplicationConstants.BusinessRules.ComplexComplexityMultiplier,
            _ => ApplicationConstants.BusinessRules.MediumComplexityMultiplier
        };

    private static decimal NormalizeFactor(decimal factor, decimal minimum, decimal maximum)
    {
        if (factor <= 0)
            return 1m;

        return Math.Clamp(factor, minimum, maximum);
    }

    private static decimal GetResidualCostFactor(decimal costOverrunFactor, decimal appliedHoursFactor)
    {
        if (costOverrunFactor <= 0 || appliedHoursFactor <= 0)
            return 1m;

        var residualCostFactor = costOverrunFactor / appliedHoursFactor;
        return Math.Clamp(
            residualCostFactor,
            ApplicationConstants.BusinessRules.MinimumResidualCostFactor,
            ApplicationConstants.BusinessRules.MaximumResidualCostFactor);
    }
}
