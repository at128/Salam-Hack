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
            ToArabicStatus(asOfUtc.HasValue ? invoice.GetEffectiveStatus(asOfUtc.Value) : invoice.Status),
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
            ToArabicStatus(asOfUtc.HasValue ? invoice.GetEffectiveStatus(asOfUtc.Value) : invoice.Status),
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
                ? i.Status == InvoiceStatus.Draft
                    ? "مسودة"
                    : "ملغاة"
                : i.TotalWithTax <= i.PaidAmount
                    ? "مدفوعة"
                    : i.DueDate < asOfUtc
                        ? "متأخرة"
                        : i.Status == InvoiceStatus.Sent
                            ? "مرسلة"
                            : i.Status == InvoiceStatus.PartiallyPaid
                                ? "مدفوعة جزئيا"
                                : i.Status == InvoiceStatus.Paid
                                    ? "مدفوعة"
                                    : i.Status == InvoiceStatus.Overdue
                                        ? "متأخرة"
                                        : "مسودة",
            i.IssueDate,
            i.DueDate,
            i.Currency);

    private static string ToArabicStatus(InvoiceStatus status)
        => status switch
        {
            InvoiceStatus.Draft => "مسودة",
            InvoiceStatus.Sent => "مرسلة",
            InvoiceStatus.PartiallyPaid => "مدفوعة جزئيا",
            InvoiceStatus.Paid => "مدفوعة",
            InvoiceStatus.Overdue => "متأخرة",
            InvoiceStatus.Cancelled => "ملغاة",
            _ => status.ToString()
        };
}
