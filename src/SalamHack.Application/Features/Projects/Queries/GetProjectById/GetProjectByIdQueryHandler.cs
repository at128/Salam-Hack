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
        var projection = await context.Projects
            .AsNoTracking()
            .Where(p => p.Id == query.ProjectId && p.UserId == query.UserId)
            .Select(p => new
            {
                Project = p,
                CustomerName = p.Customer.CustomerName,
                ServiceName = p.Service.ServiceName,
                ServiceCategory = p.Service.Category,
                ExpenseTotal = p.Expenses.Sum(e => (decimal?)e.Amount) ?? 0m
            })
            .FirstOrDefaultAsync(ct);

        if (projection is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        return projection.Project.ToDto(
            projection.CustomerName,
            projection.ServiceName,
            projection.ServiceCategory,
            projection.ExpenseTotal);
    }
}
