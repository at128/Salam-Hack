using System.Linq.Expressions;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Payments;

namespace SalamHack.Application.Features.Invoices;

internal static class InvoiceMappings
{
    public static InvoiceDto ToDto(
        this Invoice invoice,
        string projectName,
        string customerName,
        DateTimeOffset? asOfUtc = null)
        => new(
            invoice.Id,
            invoice.ProjectId,
            projectName,
            invoice.CustomerId,
            customerName,
            invoice.InvoiceNumber,
            invoice.TotalAmount,
            invoice.TaxAmount,
            invoice.TotalWithTax,
            invoice.AdvanceAmount,
            invoice.PaidAmount,
            invoice.RemainingAmount,
            invoice.AdvanceRemainingAmount,
            asOfUtc.HasValue ? invoice.GetEffectiveStatus(asOfUtc.Value) : invoice.Status,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.Notes,
            invoice.Currency,
            invoice.Payments.OrderByDescending(p => p.PaymentDate).Select(p => p.ToDto()).ToList(),
            invoice.CreatedAtUtc,
            invoice.LastModifiedUtc);

    public static InvoiceDto ToDto(this Invoice invoice, DateTimeOffset? asOfUtc = null)
        => new(
            invoice.Id,
            invoice.ProjectId,
            invoice.Project.ProjectName,
            invoice.CustomerId,
            invoice.Project.Customer.CustomerName,
            invoice.InvoiceNumber,
            invoice.TotalAmount,
            invoice.TaxAmount,
            invoice.TotalWithTax,
            invoice.AdvanceAmount,
            invoice.PaidAmount,
            invoice.RemainingAmount,
            invoice.AdvanceRemainingAmount,
            asOfUtc.HasValue ? invoice.GetEffectiveStatus(asOfUtc.Value) : invoice.Status,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.Notes,
            invoice.Currency,
            invoice.Payments.OrderByDescending(p => p.PaymentDate).Select(p => p.ToDto()).ToList(),
            invoice.CreatedAtUtc,
            invoice.LastModifiedUtc);

    public static PaymentDto ToDto(this Payment payment)
        => new(
            payment.Id,
            payment.InvoiceId,
            payment.Amount,
            payment.Method,
            payment.PaymentDate,
            payment.Notes,
            payment.Currency,
            payment.CreatedAtUtc);

    /// <summary>EF-translatable predicate selecting invoices that count as overdue at <paramref name="asOfUtc"/>.</summary>
    public static Expression<Func<Invoice, bool>> IsOverdueAtExpression(DateTimeOffset asOfUtc)
        => i => i.Status != InvoiceStatus.Draft &&
                i.Status != InvoiceStatus.Cancelled &&
                i.TotalWithTax > i.PaidAmount &&
                (i.Status == InvoiceStatus.Overdue || i.DueDate < asOfUtc);

    /// <summary>EF-translatable projection mirroring <see cref="Invoice.GetEffectiveStatus"/> + list-item shape.</summary>
    public static Expression<Func<Invoice, InvoiceListItemDto>> ToListItemExpression(DateTimeOffset asOfUtc)
        => i => new InvoiceListItemDto(
            i.Id,
            i.ProjectId,
            i.Project.ProjectName,
            i.CustomerId,
            i.Project.Customer.CustomerName,
            i.InvoiceNumber,
            i.TotalWithTax,
            i.PaidAmount,
            i.TotalWithTax - i.PaidAmount,
            i.Status == InvoiceStatus.Draft || i.Status == InvoiceStatus.Cancelled
                ? i.Status
                : i.TotalWithTax <= i.PaidAmount
                    ? InvoiceStatus.Paid
                    : i.DueDate < asOfUtc
                        ? InvoiceStatus.Overdue
                        : i.Status,
            i.IssueDate,
            i.DueDate,
            i.Currency);
}
