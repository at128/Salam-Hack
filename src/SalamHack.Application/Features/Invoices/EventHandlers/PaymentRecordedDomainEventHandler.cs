using SalamHack.Application.Common.DomainEvents;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Invoices.Events;
using SalamHack.Domain.Notifications;
using MediatR;

namespace SalamHack.Application.Features.Invoices.EventHandlers;

public sealed class PaymentRecordedDomainEventHandler(
    IAppDbContext context,
    INotificationDeliveryService deliveryService,
    TimeProvider timeProvider)
    : INotificationHandler<PaymentRecordedDomainEvent>
{
    public async Task Handle(PaymentRecordedDomainEvent notification, CancellationToken ct)
    {
        var userId = await DomainEventHandlerHelpers.GetProjectOwnerIdAsync(
            context,
            notification.ProjectId,
            ct);

        if (userId is null)
            return;

        var status = notification.IsFullyPaid ? "fully paid" : "partially paid";
        var message = $"Payment of {notification.Amount:0.##} was recorded. Invoice is now {status}.";

        var paymentNotification = Notification.CreatePaymentReceived(
            userId.Value,
            notification.InvoiceId,
            message);

        context.Notifications.Add(paymentNotification);
        try
        {
            await context.SaveChangesAsync(ct);
            
            // Instantly push the Real-time SignalR payload to the client!
            var result = await deliveryService.SendAsync(
                new NotificationDeliveryMessage(
                    paymentNotification.Id,
                    paymentNotification.UserId,
                    paymentNotification.InvoiceId,
                    paymentNotification.ProjectId,
                    paymentNotification.Type,
                    "Payment received",
                    paymentNotification.Message),
                ct);
                
            if (result.Succeeded)
            {
                paymentNotification.MarkAsSent(timeProvider.GetUtcNow());
                await context.SaveChangesAsync(ct);
            }
        }
        catch
        {
            // Notifications must never break primary flows (recording payments).
            // If persistence fails (e.g., pending migrations), we swallow to avoid returning 500.
        }
    }
}
