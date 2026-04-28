using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Queries.GetInvoices;

public sealed class GetInvoicesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetInvoicesQuery, Result<PaginatedList<InvoiceListItemDto>>>
{
    public async Task<Result<PaginatedList<InvoiceListItemDto>>> Handle(GetInvoicesQuery query, CancellationToken ct)
    {
        var invoicesQuery = context.Invoices
            .AsNoTracking()
            .Where(i => i.Project.UserId == query.UserId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            invoicesQuery = invoicesQuery.Where(i =>
                i.InvoiceNumber.Contains(search) ||
                i.Project.ProjectName.Contains(search) ||
                i.Project.Customer.CustomerName.Contains(search));
        }

        if (query.CustomerId.HasValue)
            invoicesQuery = invoicesQuery.Where(i => i.CustomerId == query.CustomerId.Value);

        if (query.ProjectId.HasValue)
            invoicesQuery = invoicesQuery.Where(i => i.ProjectId == query.ProjectId.Value);

        if (query.Status.HasValue)
            invoicesQuery = invoicesQuery.Where(i => i.Status == query.Status.Value);

        if (query.FromDate.HasValue)
            invoicesQuery = invoicesQuery.Where(i => i.IssueDate >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            invoicesQuery = invoicesQuery.Where(i => i.IssueDate <= query.ToDate.Value);

        var totalCount = await invoicesQuery.CountAsync(ct);
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await invoicesQuery
            .OrderByDescending(i => i.IssueDate)
            .ThenByDescending(i => i.InvoiceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InvoiceListItemDto(
                i.Id,
                i.ProjectId,
                i.Project.ProjectName,
                i.CustomerId,
                i.Project.Customer.CustomerName,
                i.InvoiceNumber,
                i.TotalWithTax,
                i.PaidAmount,
                i.TotalWithTax - i.PaidAmount,
                i.Status,
                i.IssueDate,
                i.DueDate,
                i.Currency))
            .ToListAsync(ct);

        return new PaginatedList<InvoiceListItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }
}
