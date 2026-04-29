namespace SalamHack.Application.Features.Analyses.Models;

public sealed record ProjectAnalysisNumbersDto(
    decimal SuggestedPrice,
    decimal BaseCost,
    decimal AdditionalExpenses,
    decimal TotalCost,
    decimal Profit,
    decimal MarginPercent,
    decimal HourlyProfit,
    decimal EstimatedHours,
    decimal ActualHours);
