using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Notifications.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Notifications.Commands.SendDueNotifications;

public sealed class SendDueNotificationsCommandHandler(
    IAppDbContext context,
    INotificationDeliveryService deliveryService,
    TimeProvider timeProvider)
    : IRequestHandler<SendDueNotificationsCommand, Result<NotificationDeliverySummaryDto>>
{
    public async Task<Result<NotificationDeliverySummaryDto>> Handle(
        SendDueNotificationsCommand cmd,
        CancellationToken ct)
    {
        var sentAt = timeProvider.GetUtcNow();
        var dueAt = cmd.DueAtUtc ?? sentAt;
        var take = Math.Clamp(cmd.Take, 1, 500);
        var notificationsQuery = context.Notifications
            .Where(n => n.SentAt == null &&
                        (n.ScheduledAt == null || n.ScheduledAt <= dueAt));

        if (cmd.UserId.HasValue)
            notificationsQuery = notificationsQuery.Where(n => n.UserId == cmd.UserId.Value);

        var notifications = await notificationsQuery
            .OrderBy(n => n.ScheduledAt ?? n.CreatedAtUtc)
            .Take(take)
            .ToListAsync(ct);

        var failures = new List<NotificationDeliveryFailureDto>();
        var sentCount = 0;

        foreach (var notification in notifications)
        {
            var result = await deliveryService.SendAsync(
                new NotificationDeliveryMessage(
                    notification.Id,
                    notification.UserId,
                    notification.InvoiceId,
                    notification.ProjectId,
                    notification.Type,
                    BuildSubject(notification.Type),
                    notification.Message),
                ct);

            if (!result.Succeeded)
            {
                failures.Add(new NotificationDeliveryFailureDto(
                    notification.Id,
                    result.FailureReason ?? "Notification delivery failed."));
                continue;
            }

            notification.MarkAsSent(sentAt);
            sentCount++;
        }

        await context.SaveChangesAsync(ct);

        return new NotificationDeliverySummaryDto(
            AttemptedCount: notifications.Count,
            SentCount: sentCount,
            FailedCount: failures.Count,
            Failures: failures);
    }

    private static string BuildSubject(Domain.Notifications.NotificationType type)
        => type switch
        {
            Domain.Notifications.NotificationType.PaymentReminder => "Payment reminder",
            Domain.Notifications.NotificationType.OverduePaymentAlert => "Overdue payment alert",
            Domain.Notifications.NotificationType.PaymentReceived => "Payment received",
            Domain.Notifications.NotificationType.InvoiceCancelled => "Invoice cancelled",
            Domain.Notifications.NotificationType.ProjectStatusChanged => "Project status changed",
            Domain.Notifications.NotificationType.ProjectHealthWarning => "Project health warning",
            Domain.Notifications.NotificationType.ExpenseSpike => "Expense spike",
            _ => "Notification"
        };
}
