namespace SalamHack.Application.Features.Invoices.Models;

public sealed record InvoiceExportDto(
    Guid InvoiceId,
    string InvoiceNumber,
    string FileName,
    string ContentType,
    byte[] Content);
