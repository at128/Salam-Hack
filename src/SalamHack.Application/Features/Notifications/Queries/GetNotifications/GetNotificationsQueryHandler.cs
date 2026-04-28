using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Notifications.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetNotificationsQuery, Result<IReadOnlyCollection<NotificationDto>>>
{
    public async Task<Result<IReadOnlyCollection<NotificationDto>>> Handle(GetNotificationsQuery query, CancellationToken ct)
    {
        var notificationsQuery = context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == query.UserId);

        if (query.IsRead.HasValue)
            notificationsQuery = notificationsQuery.Where(n => n.IsRead == query.IsRead.Value);

        if (query.Type.HasValue)
            notificationsQuery = notificationsQuery.Where(n => n.Type == query.Type.Value);

        var notifications = await notificationsQuery
            .OrderBy(n => n.IsRead)
            .ThenByDescending(n => n.ScheduledAt ?? n.CreatedAtUtc)
            .Take(query.Take)
            .Select(n => new NotificationDto(
                n.Id,
                n.UserId,
                n.InvoiceId,
                n.ProjectId,
                n.Type,
                n.Message,
                n.IsRead,
                n.ScheduledAt,
                n.SentAt,
                n.CreatedAtUtc,
                n.LastModifiedUtc))
            .ToListAsync(ct);

        return notifications;
    }
}
