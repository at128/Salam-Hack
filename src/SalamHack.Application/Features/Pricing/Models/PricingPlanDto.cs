namespace SalamHack.Application.Features.Pricing.Models;

public sealed record PricingPlanDto(
    string Name,
    decimal Price,
    decimal MarginPercent,
    decimal AdvanceAmount,
    bool IsViable);
