using SalamHack.Domain.Projects;

namespace SalamHack.Application.Features.Pricing.Models;

public sealed record PricingQuoteDto(
    Guid ServiceId,
    string ServiceName,
    ComplexityLevel Complexity,
    decimal EstimatedHours,
    decimal AdjustedHours,
    decimal RealCost,
    decimal MinAcceptablePrice,
    decimal TargetMarginPercent,
    decimal NaivePrice,
    decimal NaiveMarginPercent,
    ServiceHistoryStatsDto History,
    IReadOnlyCollection<ServiceHistoryProjectDto> RecentProjects,
    IReadOnlyCollection<PricingPlanDto> Plans,
    IReadOnlyCollection<PricingInsightDto> Insights,
    PricingAdjustmentDto Adjustments);
