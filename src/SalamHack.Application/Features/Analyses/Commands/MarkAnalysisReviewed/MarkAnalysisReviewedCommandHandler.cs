using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Analyses.Commands.MarkAnalysisReviewed;

public sealed class MarkAnalysisReviewedCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<MarkAnalysisReviewedCommand, Result<AnalysisDto>>
{
    public async Task<Result<AnalysisDto>> Handle(MarkAnalysisReviewedCommand cmd, CancellationToken ct)
    {
        var analysis = await context.Analyses
            .FirstOrDefaultAsync(a => a.Id == cmd.AnalysisId && a.Project.UserId == cmd.UserId, ct);

        if (analysis is null)
            return ApplicationErrors.Analyses.AnalysisNotFound;

        analysis.MarkReviewed(timeProvider.GetUtcNow());
        await context.SaveChangesAsync(ct);

        return analysis.ToDto();
    }
}
