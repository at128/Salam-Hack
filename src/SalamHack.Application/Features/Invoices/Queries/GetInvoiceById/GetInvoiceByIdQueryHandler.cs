using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Queries.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Project)
                .ThenInclude(p => p.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId && i.UserId == query.UserId, ct);

        return invoice is null
            ? ApplicationErrors.Invoices.InvoiceNotFound
            : invoice.ToDto(timeProvider.GetUtcNow());
    }
}
