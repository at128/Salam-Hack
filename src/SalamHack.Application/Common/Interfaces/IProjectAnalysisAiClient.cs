using SalamHack.Application.Features.Analyses.Models;

namespace SalamHack.Application.Common.Interfaces;

public interface IProjectAnalysisAiClient
{
    Task<ProjectAiAnalysisClientResult?> AnalyzeAsync(
        ProjectAiAnalysisPrompt prompt,
        CancellationToken ct = default);
}
