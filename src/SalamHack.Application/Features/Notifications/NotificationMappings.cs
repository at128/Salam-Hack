using SalamHack.Application.Features.Notifications.Models;
using SalamHack.Domain.Notifications;

namespace SalamHack.Application.Features.Notifications;

internal static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification notification)
        => new(
            notification.Id,
            notification.UserId,
            notification.InvoiceId,
            notification.ProjectId,
            notification.Type,
            notification.Message,
            notification.IsRead,
            notification.ScheduledAt,
            notification.SentAt,
            notification.CreatedAtUtc,
            notification.LastModifiedUtc);
}
