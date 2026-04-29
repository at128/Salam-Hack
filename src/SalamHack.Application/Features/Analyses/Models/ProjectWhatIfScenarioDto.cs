namespace SalamHack.Application.Features.Analyses.Models;

public enum ProjectWhatIfScenarioType
{
    PriceIncrease,
    CostReduction,
    ExtraHours
}

public sealed record ProjectWhatIfScenarioDto(
    ProjectWhatIfScenarioType Type,
    string Label,
    decimal ProjectedProfit,
    decimal? ProjectedHourlyProfit,
    decimal ChangeAmount,
    bool IsPositive);
