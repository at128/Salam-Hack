using SalamHack.Domain.Common;

namespace SalamHack.Domain.Invoices.Events;

public sealed class InvoiceOverdueDomainEvent(
    Guid invoiceId,
    Guid projectId,
    Guid customerId,
    decimal remainingAmount,
    DateTimeOffset dueDate) : DomainEvent
{
    public Guid InvoiceId { get; } = invoiceId;
    public Guid ProjectId { get; } = projectId;
    public Guid CustomerId { get; } = customerId;
    public decimal RemainingAmount { get; } = remainingAmount;
    public DateTimeOffset DueDate { get; } = dueDate;
}
