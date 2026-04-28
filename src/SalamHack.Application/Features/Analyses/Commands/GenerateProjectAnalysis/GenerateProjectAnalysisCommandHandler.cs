using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Analyses;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Analyses.Commands.GenerateProjectAnalysis;

public sealed class GenerateProjectAnalysisCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<GenerateProjectAnalysisCommand, Result<AnalysisDto>>
{
    public async Task<Result<AnalysisDto>> Handle(GenerateProjectAnalysisCommand cmd, CancellationToken ct)
    {
        var project = await context.Projects
            .Include(p => p.Expenses)
            .Include(p => p.Analyses)
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId && p.UserId == cmd.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        var health = project.GetHealthSnapshot(project.Expenses.Sum(e => e.Amount));
        if (health.IsError)
            return health.Errors;

        var narrative = ProjectAnalysisNarrative.Build(project.ProjectName, health.Value);
        var generatedAt = timeProvider.GetUtcNow();
        var analysis = project.Analyses
            .Where(a => a.Type == AnalysisType.ProjectHealth)
            .OrderByDescending(a => a.GeneratedAt)
            .FirstOrDefault();

        if (analysis is null)
        {
            var createResult = Analysis.Create(
                project.Id,
                AnalysisType.ProjectHealth,
                narrative.WhatHappened,
                narrative.WhatItMeans,
                narrative.WhatToDo,
                health.Value.HealthStatus.ToString(),
                generatedAt,
                title: $"Project health: {project.ProjectName}",
                summary: $"Margin is {health.Value.MarginPercent:0.##}%.",
                confidenceScore: 1m,
                metadataJson: null);

            if (createResult.IsError)
                return createResult.Errors;

            analysis = createResult.Value;
            context.Analyses.Add(analysis);
        }
        else
        {
            var updateResult = analysis.Update(
                AnalysisType.ProjectHealth,
                narrative.WhatHappened,
                narrative.WhatItMeans,
                narrative.WhatToDo,
                health.Value.HealthStatus.ToString(),
                generatedAt,
                title: $"Project health: {project.ProjectName}",
                summary: $"Margin is {health.Value.MarginPercent:0.##}%.",
                confidenceScore: 1m,
                metadataJson: null);

            if (updateResult.IsError)
                return updateResult.Errors;
        }

        await context.SaveChangesAsync(ct);
        return analysis.ToDto();
    }
}
