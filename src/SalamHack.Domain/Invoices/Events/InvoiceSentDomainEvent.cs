using SalamHack.Domain.Common;

namespace SalamHack.Domain.Invoices.Events;

public sealed class InvoiceSentDomainEvent(
    Guid invoiceId,
    Guid projectId,
    Guid customerId,
    decimal totalWithTax,
    DateTimeOffset dueDate) : DomainEvent
{
    public Guid InvoiceId { get; } = invoiceId;
    public Guid ProjectId { get; } = projectId;
    public Guid CustomerId { get; } = customerId;
    public decimal TotalWithTax { get; } = totalWithTax;
    public DateTimeOffset DueDate { get; } = dueDate;
}
