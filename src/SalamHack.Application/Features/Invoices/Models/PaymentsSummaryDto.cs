namespace SalamHack.Application.Features.Invoices.Models;

public sealed record PaymentsSummaryDto(
    decimal TotalInvoiced,
    decimal TotalCollected,
    decimal TotalOutstanding,
    decimal TotalOverdue,
    decimal CollectionRatePercent,
    int PaidInvoiceCount,
    int PendingInvoiceCount,
    int OverdueInvoiceCount,
    IReadOnlyCollection<PaymentsSummaryOverdueInvoiceDto> OverdueInvoices);

public sealed record PaymentsSummaryOverdueInvoiceDto(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    decimal RemainingAmount,
    DateTimeOffset DueDate,
    int DaysOverdue,
    string Currency);
