using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Analyses;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Analyses.Commands.SaveAnalysis;

public sealed record SaveAnalysisCommand(
    Guid UserId,
    Guid ProjectId,
    Guid? AnalysisId,
    AnalysisType Type,
    string WhatHappened,
    string WhatItMeans,
    string WhatToDo,
    string HealthStatus,
    DateTimeOffset GeneratedAt,
    string? Title,
    string? Summary,
    decimal? ConfidenceScore,
    string? MetadataJson) : IRequest<Result<AnalysisDto>>;
