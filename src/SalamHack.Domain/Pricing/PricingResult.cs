namespace SalamHack.Domain.Pricing;

public record PricingResult(
    decimal EconomyPrice,
    decimal MarketPrice,
    decimal PremiumPrice,
    decimal MinAcceptablePrice,
    decimal AdvanceAmount,
    decimal RealCost,
    decimal AdjustedHours,
    decimal TargetMarginPercent,
    decimal ComplexityMultiplier,
    decimal HistoricalHoursFactor,
    decimal AppliedCostFactor,
    decimal HourlyFloorPrice,
    decimal CostBasedPrice,
    decimal BaseRecommendedPrice,
    int IncludedRevisions,
    int RequestedRevisions,
    int ExtraRevisionCount,
    bool IsUrgent,
    decimal RevisionMultiplier,
    decimal UrgencyMultiplier,
    decimal ConfidenceMultiplier)
{
    public decimal EconomyMarginPercent => EconomyPrice > 0
        ? Math.Round((EconomyPrice - RealCost) / EconomyPrice * 100, 1)
        : 0;

    public decimal MarketMarginPercent => MarketPrice > 0
        ? Math.Round((MarketPrice - RealCost) / MarketPrice * 100, 1)
        : 0;

    public decimal PremiumMarginPercent => PremiumPrice > 0
        ? Math.Round((PremiumPrice - RealCost) / PremiumPrice * 100, 1)
        : 0;

    public bool IsViableAtEconomy => EconomyPrice >= MinAcceptablePrice;

    public decimal GetPrice(PricingPlanType planType)
        => planType switch
        {
            PricingPlanType.Economy => EconomyPrice,
            PricingPlanType.Recommended => MarketPrice,
            PricingPlanType.Premium => PremiumPrice,
            _ => MarketPrice
        };

    public decimal GetMarginPercent(PricingPlanType planType)
        => planType switch
        {
            PricingPlanType.Economy => EconomyMarginPercent,
            PricingPlanType.Recommended => MarketMarginPercent,
            PricingPlanType.Premium => PremiumMarginPercent,
            _ => MarketMarginPercent
        };
}
