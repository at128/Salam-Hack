namespace SalamHack.Application.Features.Notifications.Models;

public sealed record NotificationDeliveryFailureDto(
    Guid NotificationId,
    string Reason);
