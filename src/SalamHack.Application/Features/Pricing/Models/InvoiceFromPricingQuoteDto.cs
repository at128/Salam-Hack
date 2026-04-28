using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Pricing;

namespace SalamHack.Application.Features.Pricing.Models;

public sealed record InvoiceFromPricingQuoteDto(
    ProjectDto Project,
    InvoiceDto Invoice,
    PricingQuoteDto Quote,
    PricingPlanType SelectedPlan,
    decimal SelectedPrice,
    decimal AdvanceAmount);
