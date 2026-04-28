using SalamHack.Domain.Analyses;

namespace SalamHack.Application.Features.Analyses.Models;

public sealed record AnalysisDto(
    Guid Id,
    Guid ProjectId,
    AnalysisType Type,
    string WhatHappened,
    string WhatItMeans,
    string WhatToDo,
    string HealthStatus,
    DateTimeOffset GeneratedAt,
    string? Title,
    string? Summary,
    decimal? ConfidenceScore,
    string? MetadataJson,
    DateTimeOffset? ReviewedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedUtc);
