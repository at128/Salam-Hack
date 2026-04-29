using SalamHack.Domain.Notifications;

namespace SalamHack.Application.Features.Notifications.Models;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    Guid? InvoiceId,
    Guid? ProjectId,
    NotificationType Type,
    string Message,
    bool IsRead,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? SentAt,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedUtc);
