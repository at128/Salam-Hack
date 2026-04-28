using System.Globalization;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Common.DomainEvents;

internal static class DomainEventHandlerHelpers
{
    public static async Task<Guid?> GetProjectOwnerIdAsync(
        IAppDbContext context,
        Guid projectId,
        CancellationToken ct)
    {
        var userId = await context.Projects
            .Where(p => p.Id == projectId)
            .Select(p => (Guid?)p.UserId)
            .FirstOrDefaultAsync(ct);

        return userId == Guid.Empty ? null : userId;
    }

    public static async Task AddNotificationIfMissingAsync(
        IAppDbContext context,
        Notification notification,
        CancellationToken ct)
    {
        var exists = await context.Notifications.AnyAsync(n =>
            n.UserId == notification.UserId &&
            n.InvoiceId == notification.InvoiceId &&
            n.ProjectId == notification.ProjectId &&
            n.Type == notification.Type &&
            n.Message == notification.Message,
            ct);

        if (!exists)
            context.Notifications.Add(notification);
    }

    public static string FormatDate(DateTimeOffset date)
        => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
