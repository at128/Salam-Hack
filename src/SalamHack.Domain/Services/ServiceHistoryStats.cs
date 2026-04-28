namespace SalamHack.Domain.Services;

public sealed record ServiceHistoryStats(
    int CompletedProjectCount,
    decimal AverageEstimatedHours,
    decimal AverageActualHours,
    decimal HoursOverrunFactor,
    decimal CostOverrunFactor,
    decimal AverageExtraExpenses)
{
    public static ServiceHistoryStats Empty { get; } = new(
        CompletedProjectCount: 0,
        AverageEstimatedHours: 0,
        AverageActualHours: 0,
        HoursOverrunFactor: 1,
        CostOverrunFactor: 1,
        AverageExtraExpenses: 0);

    public bool HasHistory => CompletedProjectCount > 0;
}
