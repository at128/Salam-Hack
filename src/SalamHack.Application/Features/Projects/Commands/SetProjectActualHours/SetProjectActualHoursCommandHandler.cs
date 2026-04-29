using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Projects.Commands.SetProjectActualHours;

public sealed class SetProjectActualHoursCommandHandler(IAppDbContext context)
    : IRequestHandler<SetProjectActualHoursCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(SetProjectActualHoursCommand cmd, CancellationToken ct)
    {
        var project = await context.Projects
            .Include(p => p.Customer)
            .Include(p => p.Service)
            .Include(p => p.Expenses)
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId && p.UserId == cmd.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        var updateResult = project.SetActualHours(cmd.ActualHours);
        if (updateResult.IsError)
            return updateResult.Errors;

        await context.SaveChangesAsync(ct);

        return project.ToDto(project.Expenses.Sum(e => e.Amount));
    }
}
