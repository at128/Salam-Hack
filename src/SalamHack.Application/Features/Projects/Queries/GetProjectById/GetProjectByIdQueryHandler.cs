using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Projects.Queries.GetProjectById;

public sealed class GetProjectByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery query, CancellationToken ct)
    {
        var project = await context.Projects
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Service)
            .Include(p => p.Expenses)
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId && p.UserId == query.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        return project.ToDto(project.Expenses.Sum(e => e.Amount));
    }
}
