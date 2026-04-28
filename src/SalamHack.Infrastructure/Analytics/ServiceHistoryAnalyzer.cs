using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Projects;
using SalamHack.Domain.Services;
using SalamHack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Infrastructure.Analytics;

public sealed class ServiceHistoryAnalyzer(AppDbContext context) : IServiceHistoryAnalyzer
{
    public async Task<ServiceHistoryStats> AnalyzeAsync(
        Guid userId,
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        var projects = await context.Projects
            .AsNoTracking()
            .Include(p => p.Expenses)
            .Where(p => p.UserId == userId &&
                        p.ServiceId == serviceId &&
                        p.Status == ProjectStatus.Completed)
            .ToListAsync(cancellationToken);

        if (projects.Count == 0)
            return ServiceHistoryStats.Empty;

        var estimatedHours = projects.Average(p => p.EstimatedHours);
        var actualHours = projects.Average(p => p.ActualHours > 0 ? p.ActualHours : p.EstimatedHours);
        var totalEstimatedHours = projects.Sum(p => p.EstimatedHours);
        var totalActualHours = projects.Sum(p => p.ActualHours > 0 ? p.ActualHours : p.EstimatedHours);

        var estimatedCost = projects.Sum(p => Project.CalculateRealCost(p.EstimatedHours, p.ToolCost));
        var actualCost = projects.Sum(p => Project.CalculateRealCost(p.ActualHours > 0 ? p.ActualHours : p.EstimatedHours, p.ToolCost));
        var avgExtraExpenses = projects.Average(p => p.Expenses.Sum(e => e.Amount));
        var avgMarginPercent = projects.Average(p =>
        {
            var actualProjectCost =
                Project.CalculateRealCost(p.ActualHours > 0 ? p.ActualHours : p.EstimatedHours, p.ToolCost) +
                p.Expenses.Sum(e => e.Amount);

            return Project.CalculateMarginPercent(p.SuggestedPrice, actualProjectCost);
        });

        return new ServiceHistoryStats(
            CompletedProjectCount: projects.Count,
            AverageEstimatedHours: Math.Round(estimatedHours, 2),
            AverageActualHours: Math.Round(actualHours, 2),
            AverageMarginPercent: Math.Round(avgMarginPercent, 2),
            HoursOverrunFactor: totalEstimatedHours > 0 ? Math.Round(totalActualHours / totalEstimatedHours, 2) : 1,
            CostOverrunFactor: estimatedCost > 0 ? Math.Round(actualCost / estimatedCost, 2) : 1,
            AverageExtraExpenses: Math.Round(avgExtraExpenses, 2));
    }
}
