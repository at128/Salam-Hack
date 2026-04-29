using SalamHack.Application.Features.Invoices.Models;

namespace SalamHack.Application.Common.Interfaces;

public interface IInvoicePdfRenderer
{
    Task<InvoicePdfFile> RenderAsync(
        InvoiceDto invoice,
        CancellationToken cancellationToken = default);
}

public sealed record InvoicePdfFile(
    string FileName,
    string ContentType,
    byte[] Content);
