using SalamHack.Domain.Common;
using SalamHack.Domain.Payments;

namespace SalamHack.Domain.Invoices.Events;

public sealed class PaymentRecordedDomainEvent(
    Guid invoiceId,
    Guid paymentId,
    Guid projectId,
    decimal amount,
    decimal remainingAmount,
    PaymentMethod method,
    DateTimeOffset paymentDate) : DomainEvent
{
    public Guid InvoiceId { get; } = invoiceId;
    public Guid PaymentId { get; } = paymentId;
    public Guid ProjectId { get; } = projectId;
    public decimal Amount { get; } = amount;
    public decimal RemainingAmount { get; } = remainingAmount;
    public PaymentMethod Method { get; } = method;
    public DateTimeOffset PaymentDate { get; } = paymentDate;
    public bool IsFullyPaid { get; } = remainingAmount <= 0;
}
