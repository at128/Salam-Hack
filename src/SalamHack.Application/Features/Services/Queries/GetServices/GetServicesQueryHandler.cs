using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Services.Queries.GetServices;

public sealed class GetServicesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetServicesQuery, Result<PaginatedList<ServiceListItemDto>>>
{
    public async Task<Result<PaginatedList<ServiceListItemDto>>> Handle(GetServicesQuery query, CancellationToken ct)
    {
        var servicesQuery = context.Services
            .AsNoTracking()
            .Where(s => s.UserId == query.UserId);

        if (!query.IncludeInactive)
            servicesQuery = servicesQuery.Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            servicesQuery = servicesQuery.Where(s => s.ServiceName.Contains(search));
        }

        if (query.Category.HasValue)
            servicesQuery = servicesQuery.Where(s => s.Category == query.Category.Value);

        var totalCount = await servicesQuery.CountAsync(ct);
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await servicesQuery
            .OrderBy(s => s.ServiceName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ServiceListItemDto(
                s.Id,
                s.ServiceName,
                s.Category,
                s.DefaultHourlyRate,
                s.DefaultRevisions,
                s.IsActive,
                s.Projects.Count,
                s.CreatedAtUtc))
            .ToListAsync(ct);

        return new PaginatedList<ServiceListItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }
}
