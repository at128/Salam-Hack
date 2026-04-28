using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Projects.Queries.GetProjects;

public sealed class GetProjectsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetProjectsQuery, Result<PaginatedList<ProjectListItemDto>>>
{
    public async Task<Result<PaginatedList<ProjectListItemDto>>> Handle(GetProjectsQuery query, CancellationToken ct)
    {
        var projectsQuery = context.Projects
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Service)
            .Include(p => p.Expenses)
            .Where(p => p.UserId == query.UserId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            projectsQuery = projectsQuery.Where(p =>
                p.ProjectName.Contains(search) ||
                p.Customer.CustomerName.Contains(search) ||
                p.Service.ServiceName.Contains(search));
        }

        if (query.CustomerId.HasValue)
            projectsQuery = projectsQuery.Where(p => p.CustomerId == query.CustomerId.Value);

        if (query.ServiceId.HasValue)
            projectsQuery = projectsQuery.Where(p => p.ServiceId == query.ServiceId.Value);

        if (query.Status.HasValue)
            projectsQuery = projectsQuery.Where(p => p.Status == query.Status.Value);

        var totalCount = await projectsQuery.CountAsync(ct);
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var projects = await projectsQuery
            .OrderByDescending(p => p.StartDate)
            .ThenBy(p => p.ProjectName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = projects
            .Select(p => p.ToListItemDto(p.Expenses.Sum(e => e.Amount)))
            .ToList();

        return new PaginatedList<ProjectListItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }
}
