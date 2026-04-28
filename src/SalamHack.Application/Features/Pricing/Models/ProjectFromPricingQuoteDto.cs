using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Pricing;

namespace SalamHack.Application.Features.Pricing.Models;

public sealed record ProjectFromPricingQuoteDto(
    ProjectDto Project,
    PricingQuoteDto Quote,
    PricingPlanType SelectedPlan,
    decimal SelectedPrice,
    decimal AdvanceAmount);
