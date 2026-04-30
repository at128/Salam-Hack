namespace SalamHack.Application.Features.Invoices.Models;

public sealed record InvoiceDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid CustomerId,
    string CustomerName,
    string InvoiceNumber,
    decimal TotalAmount,
    decimal TaxAmount,
    decimal TotalWithTax,
    decimal AdvanceAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    decimal AdvanceRemainingAmount,
    string Status,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    string? Notes,
    string Currency,
    IReadOnlyCollection<PaymentDto> Payments,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastModifiedUtc);
