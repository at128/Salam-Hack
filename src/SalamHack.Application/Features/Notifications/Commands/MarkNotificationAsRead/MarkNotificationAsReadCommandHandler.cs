using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Notifications.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler(IAppDbContext context)
    : IRequestHandler<MarkNotificationAsReadCommand, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(MarkNotificationAsReadCommand cmd, CancellationToken ct)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == cmd.NotificationId && n.UserId == cmd.UserId, ct);

        if (notification is null)
            return ApplicationErrors.Notifications.NotificationNotFound;

        notification.MarkAsRead();
        await context.SaveChangesAsync(ct);

        return notification.ToDto();
    }
}
