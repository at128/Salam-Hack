namespace SalamHack.Application.Features.Pricing.Models;

public sealed record PricingAdjustmentDto(
    decimal ServiceHourlyRate,
    decimal ComplexityMultiplier,
    decimal HistoricalHoursFactor,
    decimal AppliedCostFactor,
    decimal HourlyFloorPrice,
    decimal CostBasedPrice,
    decimal BaseRecommendedPrice,
    int IncludedRevisions,
    int RequestedRevisions,
    int ExtraRevisions,
    bool IsUrgent,
    decimal RevisionMultiplier,
    decimal UrgencyMultiplier,
    decimal ConfidenceMultiplier);
