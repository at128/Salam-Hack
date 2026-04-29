using SalamHack.Domain.Invoices;

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
    InvoiceStatus Status,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    string Currency);
