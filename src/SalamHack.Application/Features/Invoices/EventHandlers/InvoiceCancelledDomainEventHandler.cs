using SalamHack.Application.Common.DomainEvents;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Invoices.Events;
using SalamHack.Domain.Notifications;
using MediatR;

namespace SalamHack.Application.Features.Invoices.EventHandlers;

public sealed class InvoiceCancelledDomainEventHandler(IAppDbContext context)
    : INotificationHandler<InvoiceCancelledDomainEvent>
{
    public async Task Handle(InvoiceCancelledDomainEvent notification, CancellationToken ct)
    {
        var userId = await DomainEventHandlerHelpers.GetProjectOwnerIdAsync(
            context,
            notification.ProjectId,
            ct);

        if (userId is null)
            return;

        var message = "Invoice was cancelled. Review cash-flow and any pending reminders.";

        var cancelledNotification = Notification.CreateInvoiceCancelled(
            userId.Value,
            notification.InvoiceId,
            message);

        await DomainEventHandlerHelpers.AddNotificationIfMissingAsync(context, cancelledNotification, ct);
        await context.SaveChangesAsync(ct);
    }
}
