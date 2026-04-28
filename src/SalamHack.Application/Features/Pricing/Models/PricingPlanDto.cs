using SalamHack.Domain.Pricing;

namespace SalamHack.Application.Features.Pricing.Models;

public sealed record PricingPlanDto(
    PricingPlanType PlanType,
    string Name,
    decimal Price,
    decimal MarginPercent,
    decimal AdvanceAmount,
    bool IsViable);
