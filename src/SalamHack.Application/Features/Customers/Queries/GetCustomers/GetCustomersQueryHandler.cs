using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler(IAppDbContext context)
    : IRequestHandler<GetCustomersQuery, Result<PaginatedList<CustomerListItemDto>>>
{
    public async Task<Result<PaginatedList<CustomerListItemDto>>> Handle(GetCustomersQuery query, CancellationToken ct)
    {
        var customersQuery = context.Customers
            .AsNoTracking()
            .Where(c => c.UserId == query.UserId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            customersQuery = customersQuery.Where(c =>
                c.CustomerName.Contains(search) ||
                c.Email.Contains(search) ||
                c.Phone.Contains(search));
        }

        if (query.ClientType.HasValue)
            customersQuery = customersQuery.Where(c => c.ClientType == query.ClientType.Value);

        var totalCount = await customersQuery.CountAsync(ct);
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await customersQuery
            .OrderBy(c => c.CustomerName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerListItemDto(
                c.Id,
                c.CustomerName,
                c.Email,
                c.Phone,
                c.ClientType,
                c.CompanyName,
                c.Projects.Count,
                c.CreatedAtUtc))
            .ToListAsync(ct);

        return new PaginatedList<CustomerListItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }
}
