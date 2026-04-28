namespace SalamHack.Application.Features.Notifications.Models;

public sealed record NotificationDeliverySummaryDto(
    int AttemptedCount,
    int SentCount,
    int FailedCount,
    IReadOnlyCollection<NotificationDeliveryFailureDto> Failures);
