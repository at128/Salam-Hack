using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Analyses.Queries.GetProjectAnalyses;

public sealed class GetProjectAnalysesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetProjectAnalysesQuery, Result<IReadOnlyCollection<AnalysisDto>>>
{
    public async Task<Result<IReadOnlyCollection<AnalysisDto>>> Handle(GetProjectAnalysesQuery query, CancellationToken ct)
    {
        var projectExists = await context.Projects
            .AsNoTracking()
            .AnyAsync(p => p.Id == query.ProjectId && p.UserId == query.UserId, ct);

        if (!projectExists)
            return ApplicationErrors.Projects.ProjectNotFound;

        var analysesQuery = context.Analyses
            .AsNoTracking()
            .Where(a => a.ProjectId == query.ProjectId && a.Project.UserId == query.UserId);

        if (query.Type.HasValue)
            analysesQuery = analysesQuery.Where(a => a.Type == query.Type.Value);

        var analyses = await analysesQuery
            .OrderByDescending(a => a.GeneratedAt)
            .Select(a => new AnalysisDto(
                a.Id,
                a.ProjectId,
                a.Type,
                a.WhatHappened,
                a.WhatItMeans,
                a.WhatToDo,
                a.HealthStatus,
                a.GeneratedAt,
                a.Title,
                a.Summary,
                a.ConfidenceScore,
                a.MetadataJson,
                a.ReviewedAtUtc,
                a.CreatedAtUtc,
                a.LastModifiedUtc))
            .ToListAsync(ct);

        return analyses;
    }
}
