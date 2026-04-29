using SalamHack.Application.Features.Notifications.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(
    Guid UserId,
    Guid NotificationId) : IRequest<Result<NotificationDto>>;
