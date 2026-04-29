using SalamHack.Application.Features.Analyses.Commands.GenerateProjectAnalysis;
using SalamHack.Application.Features.Analyses.Commands.MarkAnalysisReviewed;
using SalamHack.Application.Features.Analyses.Commands.SaveAnalysis;
using SalamHack.Application.Features.Analyses.Queries.GetProjectAnalyses;
using SalamHack.Application.Features.Analyses.Queries.GetProjectAnalysisDashboard;
using SalamHack.Domain.Analyses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class AnalysisController(ISender sender) : ApiController
{
    [HttpGet("dashboard")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] Guid? projectId,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetProjectAnalysisDashboardQuery(userId, projectId), ct);

        return result.Match(dashboard => OkResponse(dashboard), Problem);
    }

    [HttpGet("projects/{projectId:guid}/analyses")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetProjectAnalyses(
        Guid projectId,
        [FromQuery] AnalysisType? type,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetProjectAnalysesQuery(userId, projectId, type), ct);

        return result.Match(analyses => OkResponse(analyses), Problem);
    }

    [HttpPost("projects/{projectId:guid}/generate")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> GenerateProjectAnalysis(Guid projectId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GenerateProjectAnalysisCommand(userId, projectId), ct);

        return result.Match(analysis => OkResponse(analysis, "Analysis generated successfully."), Problem);
    }

    [HttpPost("projects/{projectId:guid}/ai")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> GenerateProjectAiAnalysis(Guid projectId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GenerateProjectAnalysisCommand(userId, projectId), ct);

        return result.Match(analysis => OkResponse(analysis, "AI analysis generated successfully."), Problem);
    }

    [HttpPost("projects/{projectId:guid}/analyses")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> SaveAnalysis(
        Guid projectId,
        [FromBody] SaveAnalysisRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new SaveAnalysisCommand(
            userId,
            projectId,
            request.AnalysisId,
            request.Type,
            request.WhatHappened,
            request.WhatItMeans,
            request.WhatToDo,
            request.HealthStatus,
            request.GeneratedAt,
            request.Title,
            request.Summary,
            request.ConfidenceScore,
            request.MetadataJson), ct);

        return result.Match(analysis => OkResponse(analysis, "Analysis saved successfully."), Problem);
    }

    [HttpPost("analyses/{analysisId:guid}/review")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> MarkReviewed(Guid analysisId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new MarkAnalysisReviewedCommand(userId, analysisId), ct);

        return result.Match(analysis => OkResponse(analysis, "Analysis marked as reviewed."), Problem);
    }
}

public sealed record SaveAnalysisRequest(
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
    string? MetadataJson);
