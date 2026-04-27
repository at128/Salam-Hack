namespace SalamHack.Domain.Pricing;

public record PricingResult(
    decimal EconomyPrice,
    decimal MarketPrice,
    decimal PremiumPrice,
    decimal MinAcceptablePrice,
    decimal AdvanceAmount,
    decimal RealCost,
    decimal AdjustedHours,
    decimal TargetMarginPercent)
{
    public decimal EconomyMarginPercent => MarketPrice > 0
        ? Math.Round((EconomyPrice - RealCost) / EconomyPrice * 100, 1)
        : 0;

    public decimal MarketMarginPercent => MarketPrice > 0
        ? Math.Round((MarketPrice - RealCost) / MarketPrice * 100, 1)
        : 0;

    public decimal PremiumMarginPercent => PremiumPrice > 0
        ? Math.Round((PremiumPrice - RealCost) / PremiumPrice * 100, 1)
        : 0;

    public bool IsViableAtEconomy => EconomyPrice >= MinAcceptablePrice;
}
