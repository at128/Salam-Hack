using SalamHack.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SalamHack.Infrastructure.Notifications.Hubs;

namespace SalamHack.Infrastructure.Notifications;

public sealed class InAppNotificationDeliveryService(
    ILogger<InAppNotificationDeliveryService> logger,
    IHubContext<NotificationHub> hubContext) : INotificationDeliveryService
{
    public async Task<NotificationDeliveryResult> SendAsync(
        NotificationDeliveryMessage message,
        CancellationToken cancellationToken = default)
    {
        // Push real-time notification to the connected client matching the specified User ID
        await hubContext.Clients.User(message.UserId.ToString())
            .SendAsync("ReceiveNotification", message, cancellationToken);
            
        logger.LogInformation(
            "Marked notification {NotificationId} for user {UserId} as delivered in-app and pushed via SignalR.",
            message.NotificationId,
            message.UserId);

        return new NotificationDeliveryResult(true);
    }
}
