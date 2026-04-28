using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Analyses;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Analyses.Commands.SaveAnalysis;

public sealed class SaveAnalysisCommandHandler(IAppDbContext context)
    : IRequestHandler<SaveAnalysisCommand, Result<AnalysisDto>>
{
    public async Task<Result<AnalysisDto>> Handle(SaveAnalysisCommand cmd, CancellationToken ct)
    {
        var projectExists = await context.Projects
            .AsNoTracking()
            .AnyAsync(p => p.Id == cmd.ProjectId && p.UserId == cmd.UserId, ct);

        if (!projectExists)
            return ApplicationErrors.Projects.ProjectNotFound;

        Analysis analysis;

        if (cmd.AnalysisId.HasValue)
        {
            var existingAnalysis = await context.Analyses
                .FirstOrDefaultAsync(a => a.Id == cmd.AnalysisId.Value &&
                                          a.ProjectId == cmd.ProjectId &&
                                          a.Project.UserId == cmd.UserId, ct);

            if (existingAnalysis is null)
                return ApplicationErrors.Analyses.AnalysisNotFound;

            analysis = existingAnalysis;
            var updateResult = analysis.Update(
                cmd.Type,
                cmd.WhatHappened,
                cmd.WhatItMeans,
                cmd.WhatToDo,
                cmd.HealthStatus,
                cmd.GeneratedAt,
                cmd.Title,
                cmd.Summary,
                cmd.ConfidenceScore,
                cmd.MetadataJson);

            if (updateResult.IsError)
                return updateResult.Errors;
        }
        else
        {
            var createResult = Analysis.Create(
                cmd.ProjectId,
                cmd.Type,
                cmd.WhatHappened,
                cmd.WhatItMeans,
                cmd.WhatToDo,
                cmd.HealthStatus,
                cmd.GeneratedAt,
                cmd.Title,
                cmd.Summary,
                cmd.ConfidenceScore,
                cmd.MetadataJson);

            if (createResult.IsError)
                return createResult.Errors;

            analysis = createResult.Value;
            context.Analyses.Add(analysis);
        }

        await context.SaveChangesAsync(ct);
        return analysis.ToDto();
    }
}
