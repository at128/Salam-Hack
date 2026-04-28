namespace SalamHack.Application.Features.Pricing.Models;

public sealed record ServiceHistoryStatsDto(
    int CompletedProjectCount,
    decimal AverageEstimatedHours,
    decimal AverageActualHours,
    decimal AverageMarginPercent,
    decimal HoursOverrunFactor,
    decimal CostOverrunFactor,
    decimal AverageExtraExpenses,
    bool HasHistory);
