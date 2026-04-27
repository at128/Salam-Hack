using SalamHack.Domain.Common;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Projects;

namespace SalamHack.Domain.Notifications;

public class Notification : AuditableEntity
{
    private Notification()
    {
    }

    private Notification(
        Guid id,
        Guid userId,
        Guid? invoiceId,
        Guid? projectId,
        NotificationType type,
        string message,
        DateTimeOffset? scheduledAt)
        : base(id)
    {
        UserId = userId;
        InvoiceId = invoiceId;
        ProjectId = projectId;
        Type = type;
        Message = message;
        IsRead = false;
        ScheduledAt = scheduledAt;
    }

    public Guid UserId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Message { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTimeOffset? ScheduledAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }

    public Invoice? Invoice { get; private set; }
    public Project? Project { get; private set; }

    public static Notification CreateReminder(
        Guid userId,
        Guid invoiceId,
        string message,
        DateTimeOffset scheduledAt)
        => CreateForInvoice(userId, invoiceId, NotificationType.PaymentReminder, message, scheduledAt);

    public static Notification CreatePaymentReminder(
        Guid userId,
        Guid invoiceId,
        string message,
        DateTimeOffset? scheduledAt = null)
        => CreateForInvoice(userId, invoiceId, NotificationType.PaymentReminder, message, scheduledAt);

    public static Notification CreateOverdueAlert(
        Guid userId,
        Guid invoiceId,
        string message)
        => CreateForInvoice(userId, invoiceId, NotificationType.OverduePaymentAlert, message, scheduledAt: null);

    public static Notification CreatePaymentReceived(
        Guid userId,
        Guid invoiceId,
        string message)
        => CreateForInvoice(userId, invoiceId, NotificationType.PaymentReceived, message, scheduledAt: null);

    public static Notification CreateInvoiceCancelled(
        Guid userId,
        Guid invoiceId,
        string message)
        => CreateForInvoice(userId, invoiceId, NotificationType.InvoiceCancelled, message, scheduledAt: null);

    public static Notification CreateProjectStatusChanged(
        Guid userId,
        Guid projectId,
        string message)
        => CreateForProject(userId, projectId, NotificationType.ProjectStatusChanged, message, scheduledAt: null);

    public void MarkAsRead()
    {
        IsRead = true;
    }

    public void MarkAsSent(DateTimeOffset sentAt)
    {
        SentAt = sentAt;
    }

    public void Reschedule(DateTimeOffset? scheduledAt)
    {
        ScheduledAt = scheduledAt;
    }

    private static Notification CreateForInvoice(
        Guid userId,
        Guid invoiceId,
        NotificationType type,
        string message,
        DateTimeOffset? scheduledAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(userId));

        if (invoiceId == Guid.Empty)
            throw new ArgumentException("Invoice id is required.", nameof(invoiceId));

        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Notification(
            Guid.CreateVersion7(),
            userId,
            invoiceId,
            projectId: null,
            type,
            message.Trim(),
            scheduledAt);
    }

    private static Notification CreateForProject(
        Guid userId,
        Guid projectId,
        NotificationType type,
        string message,
        DateTimeOffset? scheduledAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(userId));

        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));

        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Notification(
            Guid.CreateVersion7(),
            userId,
            invoiceId: null,
            projectId,
            type,
            message.Trim(),
            scheduledAt);
    }
}
