using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Projects.Commands.DeleteProject;

public sealed class DeleteProjectCommandHandler(IAppDbContext context, TimeProvider timeProvider)
    : IRequestHandler<DeleteProjectCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteProjectCommand cmd, CancellationToken ct)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId && p.UserId == cmd.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        project.Delete(timeProvider.GetUtcNow());
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
