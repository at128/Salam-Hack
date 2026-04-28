using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Queries.GetPayments;

public sealed class GetPaymentsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPaymentsQuery, Result<PaginatedList<PaymentDto>>>
{
    public async Task<Result<PaginatedList<PaymentDto>>> Handle(GetPaymentsQuery query, CancellationToken ct)
    {
        var paymentsQuery = context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.Project.UserId == query.UserId);

        if (query.InvoiceId.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.InvoiceId == query.InvoiceId.Value);

        if (query.FromDate.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= query.ToDate.Value);

        var totalCount = await paymentsQuery.CountAsync(ct);
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await paymentsQuery
            .OrderByDescending(p => p.PaymentDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentDto(
                p.Id,
                p.InvoiceId,
                p.Amount,
                p.Method,
                p.PaymentDate,
                p.Notes,
                p.Currency,
                p.CreatedAtUtc))
            .ToListAsync(ct);

        return new PaginatedList<PaymentDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }
}
