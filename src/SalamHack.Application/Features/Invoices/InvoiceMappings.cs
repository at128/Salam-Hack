using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Payments;

namespace SalamHack.Application.Features.Invoices;

internal static class InvoiceMappings
{
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
}
