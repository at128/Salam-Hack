using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Projects.Commands.RenameProject;

public sealed class RenameProjectCommandHandler(IAppDbContext context)
    : IRequestHandler<RenameProjectCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(RenameProjectCommand cmd, CancellationToken ct)
    {
        var project = await context.Projects
            .Include(p => p.Customer)
            .Include(p => p.Service)
            .Include(p => p.Expenses)
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId && p.UserId == cmd.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        var projectName = cmd.ProjectName.Trim();
        var nameExists = await context.Projects
            .AnyAsync(p => p.UserId == cmd.UserId && p.Id != cmd.ProjectId && p.ProjectName == projectName, ct);

        if (nameExists)
            return ApplicationErrors.Projects.ProjectNameAlreadyExists;

        var renameResult = project.Rename(projectName);
        if (renameResult.IsError)
            return renameResult.Errors;

        await context.SaveChangesAsync(ct);

        return project.ToDto(project.Expenses.Sum(e => e.Amount));
    }
}
