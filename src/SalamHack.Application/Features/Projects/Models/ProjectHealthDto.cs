using SalamHack.Domain.Projects;

namespace SalamHack.Application.Features.Projects.Models;

public sealed record ProjectHealthDto(
    decimal BaseCost,
    decimal AdditionalExpenses,
    decimal TotalCost,
    decimal Profit,
    decimal MarginPercent,
    decimal HourlyProfit,
    ProjectHealthStatus HealthStatus,
    bool IsHealthy);
