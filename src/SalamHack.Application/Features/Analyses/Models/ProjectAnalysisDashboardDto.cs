namespace SalamHack.Application.Features.Analyses.Models;

public sealed record ProjectAnalysisDashboardDto(
    int ProjectCount,
    int HealthyCount,
    int AtRiskCount,
    int CriticalCount,
    decimal AverageMarginPercent,
    IReadOnlyCollection<AnalysisInsightDto> MonthlyInsights,
    IReadOnlyCollection<ProjectAnalysisListItemDto> Projects,
    ProjectAnalysisDto? SelectedProject);
