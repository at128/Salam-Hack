using SalamHack.Domain.Invoices;

namespace SalamHack.Application.Features.Customers.Models;

public sealed record CustomerInvoiceSummaryDto(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid ProjectId,
    string ProjectName,
    decimal TotalWithTax,
    decimal PaidAmount,
    decimal RemainingAmount,
    InvoiceStatus Status,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    string Currency);
