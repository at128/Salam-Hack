using SalamHack.Application.Features.Notifications.Commands.CreateInvoiceReminder;
using SalamHack.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using SalamHack.Application.Features.Notifications.Commands.SendDueNotifications;
using SalamHack.Application.Features.Notifications.Queries.GetNotifications;
using SalamHack.Domain.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class NotificationsController(ISender sender) : ApiController
{
    [HttpGet]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] NotificationType? type,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetNotificationsQuery(userId, isRead, type, take), ct);

        return result.Match(notifications => OkResponse(notifications), Problem);
    }

    [HttpPost("invoice-reminders")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateInvoiceReminder(
        [FromBody] CreateInvoiceReminderRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new CreateInvoiceReminderCommand(userId, request.InvoiceId, request.Message, request.ScheduledAt),
            ct);

        return result.Match(
            notification => CreatedResponse(nameof(GetNotifications), null, notification, "Reminder scheduled successfully."),
            Problem);
    }

    [HttpPost("{notificationId:guid}/read")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new MarkNotificationAsReadCommand(userId, notificationId), ct);

        return result.Match(notification => OkResponse(notification, "Notification marked as read."), Problem);
    }

    [HttpPost("send-due")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> SendDue(
        [FromBody] SendDueNotificationsRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new SendDueNotificationsCommand(userId, request.DueAtUtc, request.Take),
            ct);

        return result.Match(summary => OkResponse(summary, "Due notifications processed successfully."), Problem);
    }
}

public sealed record CreateInvoiceReminderRequest(
    Guid InvoiceId,
    string Message,
    DateTimeOffset ScheduledAt);

public sealed record SendDueNotificationsRequest(
    DateTimeOffset? DueAtUtc,
    int Take = 100);
