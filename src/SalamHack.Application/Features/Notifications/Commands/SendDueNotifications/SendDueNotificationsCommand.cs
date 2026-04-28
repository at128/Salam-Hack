using SalamHack.Application.Features.Notifications.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Notifications.Commands.SendDueNotifications;

public sealed record SendDueNotificationsCommand(
    Guid? UserId = null,
    DateTimeOffset? DueAtUtc = null,
    int Take = 100) : IRequest<Result<NotificationDeliverySummaryDto>>;
