namespace SalamHack.Application.Features.Analyses.Models;

public sealed record ProjectAiAnalysisDto(
    string OverallStatus,
    int Score,
    string Summary,
    IReadOnlyCollection<ProjectAiAnalysisRiskDto> MainRisks,
    IReadOnlyCollection<ProjectAiAnalysisOpportunityDto> Opportunities,
    IReadOnlyCollection<ProjectAiAnalysisActionDto> RecommendedActions,
    string? ClientMessage,
    string WhatHappened,
    string WhatItMeans,
    string WhatToDo,
    string Confidence);

public sealed record ProjectAiAnalysisRiskDto(
    string Title,
    string Severity,
    string Reason);

public sealed record ProjectAiAnalysisOpportunityDto(
    string Title,
    string Impact);

public sealed record ProjectAiAnalysisActionDto(
    string Action,
    string Priority,
    string ExpectedEffect);

public sealed record ProjectAiAnalysisMetadataDto(
    bool AiGenerated,
    string Provider,
    string Model,
    ProjectAiAnalysisDto StructuredAnalysis,
    ProjectAiAnalysisInputDto Input);

public sealed record ProjectAiAnalysisInputDto(
    Guid ProjectId,
    string ProjectName,
    string CustomerName,
    string ServiceName,
    string ProjectStatus,
    bool IsUrgent,
    int Revision,
    int DefaultRevisions,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal SuggestedPrice,
    decimal MinPrice,
    decimal AdvanceAmount,
    decimal EstimatedHours,
    decimal ActualHours,
    decimal HoursUsedForHealth,
    decimal HoursOverrunPercent,
    decimal ToolCost,
    decimal BaseCost,
    decimal AdditionalExpenses,
    decimal TotalCost,
    decimal Profit,
    decimal MarginPercent,
    decimal HourlyProfit,
    string SystemHealthStatus,
    decimal HealthyMarginThreshold,
    decimal AtRiskMarginThreshold,
    ProjectAiInvoiceSummaryDto InvoiceSummary,
    IReadOnlyCollection<ProjectAiExpenseBreakdownDto> ExpenseBreakdown);

public sealed record ProjectAiInvoiceSummaryDto(
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal RemainingAmount,
    decimal OverdueAmount,
    int InvoiceCount,
    int OverdueInvoiceCount,
    DateTimeOffset? NextDueDate);

public sealed record ProjectAiExpenseBreakdownDto(
    string Category,
    decimal Amount,
    int Count);

public sealed record ProjectAiAnalysisPrompt(
    string SystemPrompt,
    string UserPrompt);

public sealed record ProjectAiAnalysisClientResult(
    ProjectAiAnalysisDto Analysis,
    string Provider,
    string Model);
