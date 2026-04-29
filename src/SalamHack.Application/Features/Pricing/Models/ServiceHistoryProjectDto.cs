namespace SalamHack.Application.Features.Pricing.Models;

public sealed record ServiceHistoryProjectDto(
    Guid ProjectId,
    string ProjectName,
    decimal EstimatedHours,
    decimal ActualHours,
    decimal SuggestedPrice,
    decimal ActualCost,
    decimal ExtraExpenses,
    decimal ActualMarginPercent,
    DateTimeOffset CompletedAt);
