using SalamHack.Application.Features.Notifications.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Notifications;
using MediatR;

namespace SalamHack.Application.Features.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid UserId,
    bool? IsRead = null,
    NotificationType? Type = null,
    int Take = 50) : IRequest<Result<IReadOnlyCollection<NotificationDto>>>;
