using System.Text.Json;
using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Projects;

namespace SalamHack.Application.Features.Analyses;

internal static class ProjectAiAnalysisFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static ProjectAiAnalysisDto BuildFallback(
        ProjectAiAnalysisInputDto input,
        ProjectNarrative narrative)
    {
        var riskSeverity = input.SystemHealthStatus switch
        {
            nameof(ProjectHealthStatus.Critical) => "Critical",
            nameof(ProjectHealthStatus.AtRisk) => "High",
            _ => "Low"
        };

        var risks = new List<ProjectAiAnalysisRiskDto>();
        if (input.MarginPercent < ApplicationConstants.BusinessRules.HealthyMarginThreshold)
        {
            risks.Add(new ProjectAiAnalysisRiskDto(
                "Profit margin pressure",
                riskSeverity,
                $"Margin is {input.MarginPercent:0.##}%, below the healthy threshold of {input.HealthyMarginThreshold:0.##}%."));
        }

        if (input.HoursOverrunPercent > 20)
        {
            risks.Add(new ProjectAiAnalysisRiskDto(
                "Scope or time overrun",
                "High",
                $"Actual hours are {input.HoursOverrunPercent:0.##}% above the estimate."));
        }

        if (input.InvoiceSummary.OverdueAmount > 0)
        {
            risks.Add(new ProjectAiAnalysisRiskDto(
                "Collection risk",
                "High",
                $"Overdue amount is {input.InvoiceSummary.OverdueAmount:0.##}."));
        }

        if (risks.Count == 0)
        {
            risks.Add(new ProjectAiAnalysisRiskDto(
                "No major profitability risk",
                "Low",
                "The current numbers are above the healthy margin threshold."));
        }

        var opportunities = new[]
        {
            new ProjectAiAnalysisOpportunityDto(
                "Use this project as pricing evidence",
                input.MarginPercent >= input.HealthyMarginThreshold
                    ? "It can guide future quotes for similar work."
                    : "It shows where pricing or scope controls need to improve.")
        };

        var actions = BuildFallbackActions(input);

        return new ProjectAiAnalysisDto(
            input.SystemHealthStatus,
            CalculateScore(input.MarginPercent),
            $"{input.ProjectName} margin is {input.MarginPercent:0.##}% with profit of {input.Profit:0.##}.",
            risks,
            opportunities,
            actions,
            BuildClientMessage(input),
            narrative.WhatHappened,
            narrative.WhatItMeans,
            narrative.WhatToDo,
            "High");
    }

    public static ProjectAiAnalysisDto Normalize(
        ProjectAiAnalysisDto? analysis,
        ProjectAiAnalysisInputDto input,
        ProjectNarrative fallbackNarrative)
    {
        var fallback = BuildFallback(input, fallbackNarrative);
        if (analysis is null)
            return fallback;

        var overallStatus = NormalizeStatus(analysis.OverallStatus, input);
        var score = Math.Clamp(analysis.Score, 0, 100);

        return new ProjectAiAnalysisDto(
            overallStatus,
            score,
            NormalizeText(analysis.Summary, fallback.Summary),
            NormalizeList(analysis.MainRisks, fallback.MainRisks),
            NormalizeList(analysis.Opportunities, fallback.Opportunities),
            NormalizeList(analysis.RecommendedActions, fallback.RecommendedActions),
            NormalizeOptionalText(analysis.ClientMessage, fallback.ClientMessage),
            NormalizeText(analysis.WhatHappened, fallback.WhatHappened),
            NormalizeText(analysis.WhatItMeans, fallback.WhatItMeans),
            NormalizeText(analysis.WhatToDo, fallback.WhatToDo),
            NormalizeConfidence(analysis.Confidence));
    }

    public static ProjectAiAnalysisDto? TryParseMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return null;

        try
        {
            var metadata = JsonSerializer.Deserialize<ProjectAiAnalysisMetadataDto>(metadataJson, JsonOptions);
            return metadata?.StructuredAnalysis;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static IReadOnlyCollection<ProjectAiAnalysisActionDto> BuildFallbackActions(ProjectAiAnalysisInputDto input)
    {
        var actions = new List<ProjectAiAnalysisActionDto>();

        if (input.MarginPercent < input.HealthyMarginThreshold)
        {
            actions.Add(new ProjectAiAnalysisActionDto(
                "Review pricing before repeating similar work",
                "High",
                "Protects the next project from the same margin pressure."));
        }

        if (input.HoursOverrunPercent > 20)
        {
            actions.Add(new ProjectAiAnalysisActionDto(
                "Document extra scope and approve change requests earlier",
                "High",
                "Reduces unpaid hours and protects hourly profit."));
        }

        if (input.InvoiceSummary.RemainingAmount > 0)
        {
            actions.Add(new ProjectAiAnalysisActionDto(
                "Follow up on remaining invoice balance",
                input.InvoiceSummary.OverdueAmount > 0 ? "High" : "Medium",
                "Improves cashflow and reduces collection risk."));
        }

        if (actions.Count == 0)
        {
            actions.Add(new ProjectAiAnalysisActionDto(
                "Keep the current pricing and delivery pattern",
                "Medium",
                "Maintains healthy profitability on similar projects."));
        }

        return actions;
    }

    private static string? BuildClientMessage(ProjectAiAnalysisInputDto input)
        => input.HoursOverrunPercent > 20
            ? "The project required more time than initially estimated, so any additional scope should be confirmed before continuing."
            : null;

    private static int CalculateScore(decimal marginPercent)
    {
        if (marginPercent <= 0)
            return 10;

        if (marginPercent < ApplicationConstants.BusinessRules.AtRiskMarginThreshold)
            return (int)Math.Clamp(Math.Round(marginPercent * 2), 0, 44);

        if (marginPercent < ApplicationConstants.BusinessRules.HealthyMarginThreshold)
            return (int)Math.Clamp(Math.Round(45 + (marginPercent - 15) * 2), 45, 74);

        return (int)Math.Clamp(Math.Round(75 + Math.Min(marginPercent - 30, 25)), 75, 100);
    }

    private static string NormalizeStatus(string? value, ProjectAiAnalysisInputDto input)
    {
        var requiredStatus = GetRequiredStatus(input);
        var candidate = string.Equals(value, nameof(ProjectHealthStatus.Healthy), StringComparison.OrdinalIgnoreCase)
            ? nameof(ProjectHealthStatus.Healthy)
            : string.Equals(value, nameof(ProjectHealthStatus.AtRisk), StringComparison.OrdinalIgnoreCase)
                ? nameof(ProjectHealthStatus.AtRisk)
                : string.Equals(value, nameof(ProjectHealthStatus.Critical), StringComparison.OrdinalIgnoreCase)
                    ? nameof(ProjectHealthStatus.Critical)
                    : input.SystemHealthStatus;

        return SeverityRank(candidate) >= SeverityRank(requiredStatus)
            ? candidate
            : requiredStatus;
    }

    private static string GetRequiredStatus(ProjectAiAnalysisInputDto input)
    {
        if (input.MarginPercent < ApplicationConstants.BusinessRules.AtRiskMarginThreshold)
            return nameof(ProjectHealthStatus.Critical);

        if (input.MarginPercent < ApplicationConstants.BusinessRules.HealthyMarginThreshold)
            return nameof(ProjectHealthStatus.AtRisk);

        return input.SystemHealthStatus;
    }

    private static int SeverityRank(string status)
        => status switch
        {
            nameof(ProjectHealthStatus.Critical) => 3,
            nameof(ProjectHealthStatus.AtRisk) => 2,
            _ => 1
        };

    private static string NormalizeConfidence(string? value)
        => string.Equals(value, "Low", StringComparison.OrdinalIgnoreCase)
            ? "Low"
            : string.Equals(value, "Medium", StringComparison.OrdinalIgnoreCase)
                ? "Medium"
                : "High";

    private static string NormalizeText(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string? NormalizeOptionalText(string? value, string? fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static IReadOnlyCollection<T> NormalizeList<T>(
        IReadOnlyCollection<T>? values,
        IReadOnlyCollection<T> fallback)
        => values is null || values.Count == 0 ? fallback : values;
}
