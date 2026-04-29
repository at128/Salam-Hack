using System.Text.Json;
using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Analyses;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Analyses.Commands.GenerateProjectAnalysis;

public sealed class GenerateProjectAnalysisCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider,
    IProjectAnalysisAiClient aiClient)
    : IRequestHandler<GenerateProjectAnalysisCommand, Result<AnalysisDto>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<AnalysisDto>> Handle(GenerateProjectAnalysisCommand cmd, CancellationToken ct)
    {
        var project = await context.Projects
            .Include(p => p.Customer)
            .Include(p => p.Service)
            .Include(p => p.Expenses)
            .Include(p => p.Analyses)
            .Include(p => p.Invoices)
                .ThenInclude(i => i.Payments)
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId && p.UserId == cmd.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        var health = project.GetHealthSnapshot(project.Expenses.Sum(e => e.Amount));
        if (health.IsError)
            return health.Errors;

        var generatedAt = timeProvider.GetUtcNow();
        var narrative = ProjectAnalysisNarrative.Build(project.ProjectName, health.Value);
        var aiInput = BuildAiInput(project, health.Value, generatedAt);
        var prompt = ProjectAnalysisAiPromptBuilder.Build(aiInput);
        var aiResult = await aiClient.AnalyzeAsync(prompt, ct);
        var structuredAnalysis = ProjectAiAnalysisFactory.Normalize(
            aiResult?.Analysis,
            aiInput,
            narrative);
        var metadata = new ProjectAiAnalysisMetadataDto(
            AiGenerated: aiResult is not null,
            Provider: aiResult?.Provider ?? "RuleBased",
            Model: aiResult?.Model ?? "local-fallback",
            StructuredAnalysis: structuredAnalysis,
            Input: aiInput);
        var metadataJson = JsonSerializer.Serialize(metadata, JsonOptions);

        var analysis = project.Analyses
            .Where(a => a.Type == AnalysisType.ProjectHealth)
            .OrderByDescending(a => a.GeneratedAt)
            .FirstOrDefault();

        if (analysis is null)
        {
            var createResult = Analysis.Create(
                project.Id,
                AnalysisType.ProjectHealth,
                structuredAnalysis.WhatHappened,
                structuredAnalysis.WhatItMeans,
                structuredAnalysis.WhatToDo,
                structuredAnalysis.OverallStatus,
                generatedAt,
                title: $"AI project health: {project.ProjectName}",
                summary: structuredAnalysis.Summary,
                confidenceScore: ToConfidenceScore(structuredAnalysis.Confidence),
                metadataJson: metadataJson);

            if (createResult.IsError)
                return createResult.Errors;

            analysis = createResult.Value;
            context.Analyses.Add(analysis);
        }
        else
        {
            var updateResult = analysis.Update(
                AnalysisType.ProjectHealth,
                structuredAnalysis.WhatHappened,
                structuredAnalysis.WhatItMeans,
                structuredAnalysis.WhatToDo,
                structuredAnalysis.OverallStatus,
                generatedAt,
                title: $"AI project health: {project.ProjectName}",
                summary: structuredAnalysis.Summary,
                confidenceScore: ToConfidenceScore(structuredAnalysis.Confidence),
                metadataJson: metadataJson);

            if (updateResult.IsError)
                return updateResult.Errors;
        }

        await context.SaveChangesAsync(ct);
        return analysis.ToDto();
    }

    private static ProjectAiAnalysisInputDto BuildAiInput(
        Project project,
        ProjectHealthSnapshot health,
        DateTimeOffset asOfUtc)
    {
        var hoursUsedForHealth = project.ActualHours > 0
            ? project.ActualHours
            : project.EstimatedHours;
        var hoursOverrunPercent = project.EstimatedHours > 0 && project.ActualHours > 0
            ? Math.Round((project.ActualHours - project.EstimatedHours) / project.EstimatedHours * 100, 2)
            : 0;

        var expenses = project.Expenses
            .GroupBy(e => e.Category)
            .Select(g => new ProjectAiExpenseBreakdownDto(
                g.Key.ToString(),
                Math.Round(g.Sum(e => e.Amount), 2),
                g.Count()))
            .OrderByDescending(e => e.Amount)
            .ToList();

        return new ProjectAiAnalysisInputDto(
            project.Id,
            project.ProjectName,
            project.Customer.CustomerName,
            project.Service.ServiceName,
            project.Status.ToString(),
            project.IsUrgent,
            project.Revision,
            project.Service.DefaultRevisions,
            project.StartDate,
            project.EndDate,
            project.SuggestedPrice,
            project.MinPrice,
            project.AdvanceAmount,
            project.EstimatedHours,
            project.ActualHours,
            hoursUsedForHealth,
            hoursOverrunPercent,
            project.ToolCost,
            health.BaseCost,
            health.AdditionalExpenses,
            health.TotalCost,
            health.Profit,
            health.MarginPercent,
            health.HourlyProfit,
            health.HealthStatus.ToString(),
            ApplicationConstants.BusinessRules.HealthyMarginThreshold,
            ApplicationConstants.BusinessRules.AtRiskMarginThreshold,
            BuildInvoiceSummary(project.Invoices, asOfUtc),
            expenses);
    }

    private static ProjectAiInvoiceSummaryDto BuildInvoiceSummary(
        IEnumerable<Invoice> invoices,
        DateTimeOffset asOfUtc)
    {
        var activeInvoices = invoices
            .Where(i => i.GetEffectiveStatus(asOfUtc) != InvoiceStatus.Cancelled)
            .ToList();

        var openInvoices = activeInvoices
            .Where(i => !i.IsFullyPaid)
            .ToList();

        var overdueInvoices = activeInvoices
            .Where(i => i.GetEffectiveStatus(asOfUtc) == InvoiceStatus.Overdue)
            .ToList();

        return new ProjectAiInvoiceSummaryDto(
            Math.Round(activeInvoices.Sum(i => i.TotalWithTax), 2),
            Math.Round(activeInvoices.Sum(i => i.PaidAmount), 2),
            Math.Round(activeInvoices.Sum(i => i.RemainingAmount), 2),
            Math.Round(overdueInvoices.Sum(i => i.RemainingAmount), 2),
            activeInvoices.Count,
            overdueInvoices.Count,
            openInvoices
                .OrderBy(i => i.DueDate)
                .Select(i => (DateTimeOffset?)i.DueDate)
                .FirstOrDefault());
    }

    private static decimal ToConfidenceScore(string confidence)
        => confidence.Equals("Low", StringComparison.OrdinalIgnoreCase)
            ? 0.4m
            : confidence.Equals("Medium", StringComparison.OrdinalIgnoreCase)
                ? 0.7m
                : 0.95m;
}
