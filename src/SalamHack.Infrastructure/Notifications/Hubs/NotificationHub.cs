using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SalamHack.Infrastructure.Notifications.Hubs;

[Authorize]
public sealed class NotificationHub(ILogger<NotificationHub> logger) : Hub
{
    public override Task OnConnectedAsync()
    {
        logger.LogInformation(
            "NotificationHub connected. ConnectionId={ConnectionId}, UserId={UserId}",
            Context.ConnectionId,
            Context.UserIdentifier);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation(
            exception,
            "NotificationHub disconnected. ConnectionId={ConnectionId}, UserId={UserId}",
            Context.ConnectionId,
            Context.UserIdentifier);

        return base.OnDisconnectedAsync(exception);
    }
}