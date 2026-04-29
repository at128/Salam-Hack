using SalamHack.Domain.Analyses;

namespace SalamHack.Application.Features.Analyses.Models;

public enum AnalysisInsightSeverity
{
    Info,
    Success,
    Warning,
    Critical
}

public sealed record AnalysisInsightDto(
    AnalysisType Type,
    AnalysisInsightSeverity Severity,
    string Title,
    string Summary);
