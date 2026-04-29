namespace SalamHack.Application.Features.Reports.Models;

public sealed record CashFlowPendingInvoiceDto(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    decimal RemainingAmount,
    DateTimeOffset DueDate,
    bool IsOverdue,
    string Currency);
