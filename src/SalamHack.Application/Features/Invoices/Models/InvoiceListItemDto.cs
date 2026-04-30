namespace SalamHack.Application.Features.Invoices.Models;

public sealed record InvoiceListItemDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid CustomerId,
    string CustomerName,
    string InvoiceNumber,
    decimal TotalWithTax,
    decimal PaidAmount,
    decimal RemainingAmount,
    string Status,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    string Currency);
