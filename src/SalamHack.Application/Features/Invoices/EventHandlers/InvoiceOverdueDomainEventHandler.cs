using SalamHack.Application.Common.DomainEvents;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Invoices.Events;
using SalamHack.Domain.Notifications;
using MediatR;

namespace SalamHack.Application.Features.Invoices.EventHandlers;

public sealed class InvoiceOverdueDomainEventHandler(IAppDbContext context)
    : INotificationHandler<InvoiceOverdueDomainEvent>
{
    public async Task Handle(InvoiceOverdueDomainEvent notification, CancellationToken ct)
    {
        var userId = await DomainEventHandlerHelpers.GetProjectOwnerIdAsync(
            context,
            notification.ProjectId,
            ct);

        if (userId is null)
            return;

        var dueDate = DomainEventHandlerHelpers.FormatDate(notification.DueDate);
        var message = $"Invoice is overdue since {dueDate}. Remaining amount: {notification.RemainingAmount:0.##}.";

        var overdueNotification = Notification.CreateOverdueAlert(
            userId.Value,
            notification.InvoiceId,
            message);

        await DomainEventHandlerHelpers.AddNotificationIfMissingAsync(context, overdueNotification, ct);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch
        {
            // Avoid failing the main request if notifications cannot be saved.
        }
    }
}
