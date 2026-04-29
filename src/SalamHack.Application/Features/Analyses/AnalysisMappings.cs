using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Analyses;

namespace SalamHack.Application.Features.Analyses;

internal static class AnalysisMappings
{
    public static AnalysisDto ToDto(this Analysis analysis)
        => new(
            analysis.Id,
            analysis.ProjectId,
            analysis.Type,
            analysis.WhatHappened,
            analysis.WhatItMeans,
            analysis.WhatToDo,
            analysis.HealthStatus,
            analysis.GeneratedAt,
            analysis.Title,
            analysis.Summary,
            analysis.ConfidenceScore,
            analysis.MetadataJson,
            analysis.ReviewedAtUtc,
            analysis.CreatedAtUtc,
            analysis.LastModifiedUtc,
            ProjectAiAnalysisFactory.TryParseMetadata(analysis.MetadataJson));
}
