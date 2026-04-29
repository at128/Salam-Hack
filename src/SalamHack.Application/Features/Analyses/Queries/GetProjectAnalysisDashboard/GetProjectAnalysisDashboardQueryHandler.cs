using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Analyses;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Pricing;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Analyses.Queries.GetProjectAnalysisDashboard;

public sealed class GetProjectAnalysisDashboardQueryHandler(IAppDbContext context)
    : IRequestHandler<GetProjectAnalysisDashboardQuery, Result<ProjectAnalysisDashboardDto>>
{
    public async Task<Result<ProjectAnalysisDashboardDto>> Handle(GetProjectAnalysisDashboardQuery query, CancellationToken ct)
    {
        var projects = await context.Projects
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Service)
            .Include(p => p.Expenses)
            .Include(p => p.Analyses)
            .Where(p => p.UserId == query.UserId && p.Status != ProjectStatus.Cancelled)
            .ToListAsync(ct);

        if (query.ProjectId.HasValue && projects.All(p => p.Id != query.ProjectId.Value))
            return ApplicationErrors.Projects.ProjectNotFound;

        var rows = projects
            .Select(BuildHealthRow)
            .OrderBy(r => r.Health.MarginPercent)
            .ToList();

        if (rows.Count == 0)
        {
            return new ProjectAnalysisDashboardDto(
                ProjectCount: 0,
                HealthyCount: 0,
                AtRiskCount: 0,
                CriticalCount: 0,
                AverageMarginPercent: 0,
                MonthlyInsights: [],
                Projects: [],
                SelectedProject: null);
        }

        var selectedRow = query.ProjectId.HasValue
            ? rows.First(r => r.Project.Id == query.ProjectId.Value)
            : rows.First();

        var listItems = rows
            .OrderBy(r => r.Project.ProjectName)
            .Select(r => new ProjectAnalysisListItemDto(
                r.Project.Id,
                r.Project.ProjectName,
                r.Project.CustomerId,
                r.Project.Customer.CustomerName,
                r.Health.MarginPercent,
                r.Health.HealthStatus,
                r.Health.Profit))
            .ToList();

        return new ProjectAnalysisDashboardDto(
            ProjectCount: rows.Count,
            HealthyCount: rows.Count(r => r.Health.HealthStatus == ProjectHealthStatus.Healthy),
            AtRiskCount: rows.Count(r => r.Health.HealthStatus == ProjectHealthStatus.AtRisk),
            CriticalCount: rows.Count(r => r.Health.HealthStatus == ProjectHealthStatus.Critical),
            AverageMarginPercent: Math.Round(rows.Average(r => r.Health.MarginPercent), 2),
            MonthlyInsights: BuildInsights(rows),
            Projects: listItems,
            SelectedProject: BuildSelectedProject(selectedRow));
    }

    private static ProjectAnalysisDto BuildSelectedProject(ProjectHealthRow row)
    {
        var latestStoredAnalysis = row.Project.Analyses
            .Where(a => a.Type == AnalysisType.ProjectHealth)
            .OrderByDescending(a => a.GeneratedAt)
            .FirstOrDefault();

        var narrative = latestStoredAnalysis is null
            ? ProjectAnalysisNarrative.Build(row.Project.ProjectName, row.Health)
            : new ProjectNarrative(
                latestStoredAnalysis.WhatHappened,
                latestStoredAnalysis.WhatItMeans,
                latestStoredAnalysis.WhatToDo);

        var hoursForHealth = row.Project.ActualHours > 0
            ? row.Project.ActualHours
            : row.Project.EstimatedHours;

        return new ProjectAnalysisDto(
            row.Project.Id,
            row.Project.ProjectName,
            row.Project.CustomerId,
            row.Project.Customer.CustomerName,
            row.Project.ServiceId,
            row.Project.Service.ServiceName,
            row.Health.HealthStatus,
            narrative.WhatHappened,
            narrative.WhatItMeans,
            narrative.WhatToDo,
            new ProjectAnalysisNumbersDto(
                row.Project.SuggestedPrice,
                row.Health.BaseCost,
                row.Health.AdditionalExpenses,
                row.Health.TotalCost,
                row.Health.Profit,
                row.Health.MarginPercent,
                row.Health.HourlyProfit,
                row.Project.EstimatedHours,
                row.Project.ActualHours),
            BuildScenarios(row.Project.SuggestedPrice, row.Health, hoursForHealth));
    }

    private static IReadOnlyCollection<ProjectWhatIfScenarioDto> BuildScenarios(
        decimal currentPrice,
        ProjectHealthSnapshot health,
        decimal currentHours)
    {
        var profitAfterPriceIncrease = PricingCalculator.CalculateProfitAfterPriceChange(
            currentPrice,
            health.TotalCost,
            15);
        var profitAfterCostReduction = PricingCalculator.CalculateProfitAfterCostReduction(
            currentPrice,
            health.TotalCost,
            20);
        var hourlyAfterExtraHours = PricingCalculator.CalculateHourlyProfitAfterExtraHours(
            health.Profit,
            currentHours,
            5);

        return
        [
            new ProjectWhatIfScenarioDto(
                ProjectWhatIfScenarioType.PriceIncrease,
                "Increase price by 15%",
                profitAfterPriceIncrease,
                null,
                profitAfterPriceIncrease - health.Profit,
                profitAfterPriceIncrease >= health.Profit),
            new ProjectWhatIfScenarioDto(
                ProjectWhatIfScenarioType.CostReduction,
                "Reduce costs by 20%",
                profitAfterCostReduction,
                null,
                profitAfterCostReduction - health.Profit,
                profitAfterCostReduction >= health.Profit),
            new ProjectWhatIfScenarioDto(
                ProjectWhatIfScenarioType.ExtraHours,
                "Add 5 work hours",
                health.Profit,
                hourlyAfterExtraHours,
                hourlyAfterExtraHours - health.HourlyProfit,
                hourlyAfterExtraHours >= health.HourlyProfit)
        ];
    }

    private static IReadOnlyCollection<AnalysisInsightDto> BuildInsights(IReadOnlyCollection<ProjectHealthRow> rows)
    {
        var insights = new List<AnalysisInsightDto>();
        var worst = rows.OrderBy(r => r.Health.MarginPercent).First();
        var best = rows.OrderByDescending(r => r.Health.MarginPercent).First();
        var expenseHeavy = rows
            .Where(r => r.Project.SuggestedPrice > 0)
            .OrderByDescending(r => r.Health.AdditionalExpenses / r.Project.SuggestedPrice)
            .FirstOrDefault();

        insights.Add(new AnalysisInsightDto(
            AnalysisType.ProjectHealth,
            worst.Health.HealthStatus == ProjectHealthStatus.Critical
                ? AnalysisInsightSeverity.Critical
                : AnalysisInsightSeverity.Warning,
            "Lowest project margin",
            $"{worst.Project.ProjectName} is at {worst.Health.MarginPercent}% margin."));

        insights.Add(new AnalysisInsightDto(
            AnalysisType.GeneralInsight,
            AnalysisInsightSeverity.Success,
            "Best project margin",
            $"{best.Project.ProjectName} leads with {best.Health.MarginPercent}% margin."));

        if (expenseHeavy is not null && expenseHeavy.Health.AdditionalExpenses > 0)
        {
            insights.Add(new AnalysisInsightDto(
                AnalysisType.ExpenseTrend,
                AnalysisInsightSeverity.Info,
                "Highest extra expense pressure",
                $"{expenseHeavy.Project.ProjectName} has {expenseHeavy.Health.AdditionalExpenses:0.##} in extra expenses."));
        }

        return insights;
    }

    private static ProjectHealthRow BuildHealthRow(Project project)
    {
        var health = project.GetHealthSnapshot(project.Expenses.Sum(e => e.Amount));

        return health.IsError
            ? new ProjectHealthRow(
                project,
                new ProjectHealthSnapshot(0, project.Expenses.Sum(e => e.Amount), 0, 0, 0, 0, ProjectHealthStatus.Critical))
            : new ProjectHealthRow(project, health.Value);
    }

    private sealed record ProjectHealthRow(Project Project, ProjectHealthSnapshot Health);
}
