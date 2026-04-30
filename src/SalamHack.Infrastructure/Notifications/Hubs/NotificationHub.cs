using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SalamHack.Infrastructure.Notifications.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    // Connection happens using the bearer token standard logic.
}
