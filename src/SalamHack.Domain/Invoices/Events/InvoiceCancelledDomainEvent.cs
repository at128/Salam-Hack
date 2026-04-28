using SalamHack.Domain.Common;

namespace SalamHack.Domain.Invoices.Events;

public sealed class InvoiceCancelledDomainEvent(Guid invoiceId, Guid projectId) : DomainEvent
{
    public Guid InvoiceId { get; } = invoiceId;
    public Guid ProjectId { get; } = projectId;
}
