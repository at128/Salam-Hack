using SalamHack.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace SalamHack.Infrastructure.Notifications;

public sealed class InAppNotificationDeliveryService(
    ILogger<InAppNotificationDeliveryService> logger) : INotificationDeliveryService
{
    public Task<NotificationDeliveryResult> SendAsync(
        NotificationDeliveryMessage message,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Marked notification {NotificationId} for user {UserId} as delivered in-app.",
            message.NotificationId,
            message.UserId);

        return Task.FromResult(new NotificationDeliveryResult(true));
    }
}
