using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Queries.ExportInvoicePdf;

public sealed class ExportInvoicePdfQueryHandler(
    IAppDbContext context,
    IInvoicePdfRenderer renderer,
    TimeProvider timeProvider)
    : IRequestHandler<ExportInvoicePdfQuery, Result<InvoiceExportDto>>
{
    public async Task<Result<InvoiceExportDto>> Handle(ExportInvoicePdfQuery query, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Project)
                .ThenInclude(p => p.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId && i.UserId == query.UserId, ct);

        if (invoice is null)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        var invoiceDto = invoice.ToDto(timeProvider.GetUtcNow());
        var file = await renderer.RenderAsync(invoiceDto, ct);

        return new InvoiceExportDto(
            invoiceDto.Id,
            invoiceDto.InvoiceNumber,
            file.FileName,
            file.ContentType,
            file.Content);
    }
}
