namespace SalamHack.Domain.Projects;

public sealed record ProjectHealthSnapshot(
    decimal BaseCost,
    decimal AdditionalExpenses,
    decimal TotalCost,
    decimal Profit,
    decimal MarginPercent,
    decimal HourlyProfit,
    ProjectHealthStatus HealthStatus)
{
    public bool IsHealthy => HealthStatus == ProjectHealthStatus.Healthy;

    public decimal MarginDeltaFromBenchmark(decimal benchmarkMarginPercent)
        => Math.Round(MarginPercent - benchmarkMarginPercent, 2);
}
