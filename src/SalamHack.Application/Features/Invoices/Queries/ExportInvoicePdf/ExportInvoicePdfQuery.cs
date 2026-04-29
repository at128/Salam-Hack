using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Queries.ExportInvoicePdf;

public sealed record ExportInvoicePdfQuery(
    Guid UserId,
    Guid InvoiceId) : IRequest<Result<InvoiceExportDto>>;
