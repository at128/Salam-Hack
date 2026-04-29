using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Projects.Commands.ChangeProjectStatus;

public sealed class ChangeProjectStatusCommandHandler(IAppDbContext context)
    : IRequestHandler<ChangeProjectStatusCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(ChangeProjectStatusCommand cmd, CancellationToken ct)
    {
        var project = await context.Projects
            .Include(p => p.Customer)
            .Include(p => p.Service)
            .Include(p => p.Expenses)
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId && p.UserId == cmd.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        var transitionResult = cmd.Status switch
        {
            ProjectStatus.InProgress => project.Start(),
            ProjectStatus.Completed => project.Complete(),
            ProjectStatus.Cancelled => project.Cancel(),
            _ => ApplicationErrors.Projects.UnsupportedStatusTransition
        };

        if (transitionResult.IsError)
            return transitionResult.Errors;

        await context.SaveChangesAsync(ct);

        return project.ToDto(project.Expenses.Sum(e => e.Amount));
    }
}
