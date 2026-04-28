using SalamHack.Domain.Notifications;

namespace SalamHack.Application.Common.Interfaces;

public interface INotificationDeliveryService
{
    Task<NotificationDeliveryResult> SendAsync(
        NotificationDeliveryMessage message,
        CancellationToken cancellationToken = default);
}

public sealed record NotificationDeliveryMessage(
    Guid NotificationId,
    Guid UserId,
    Guid? InvoiceId,
    Guid? ProjectId,
    NotificationType Type,
    string Subject,
    string Body);

public sealed record NotificationDeliveryResult(
    bool Succeeded,
    string? FailureReason = null);
