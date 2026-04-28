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
        decimal avgExtraExpenses)
    {
        var complexityMultiplier = GetComplexityAdjustmentFactor(complexity);
        var adjustedHours = Math.Round(estimatedHours * hoursOverrunFactor * complexityMultiplier, 1);
        var baseCost = adjustedHours * ApplicationConstants.BusinessRules.CostRatePerHour * costOverrunFactor;
        var realCost = Math.Round(baseCost + avgExtraExpenses, 2);

        var marketPrice = Math.Round(realCost / (1 - ApplicationConstants.BusinessRules.TargetProfitMarginRate), 2);
        var economyPrice = Math.Round(marketPrice * ApplicationConstants.BusinessRules.EconomyPriceMultiplier, 2);
        var premiumPrice = Math.Round(marketPrice * ApplicationConstants.BusinessRules.PremiumPriceMultiplier, 2);
        var minAcceptable = Math.Round(realCost * ApplicationConstants.BusinessRules.MinimumPriceMultiplier, 2);
        var advanceAmount = Math.Round(marketPrice * ApplicationConstants.BusinessRules.AdvancePaymentRate, 2);

        return new PricingResult(
            EconomyPrice: economyPrice,
            MarketPrice: marketPrice,
            PremiumPrice: premiumPrice,
            MinAcceptablePrice: minAcceptable,
            AdvanceAmount: advanceAmount,
            RealCost: realCost,
            AdjustedHours: adjustedHours,
            TargetMarginPercent: ApplicationConstants.BusinessRules.TargetProfitMarginRate * 100);
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
}
