using SalamHack.Application.Features.Notifications.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Notifications.Commands.CreateInvoiceReminder;

public sealed record CreateInvoiceReminderCommand(
    Guid UserId,
    Guid InvoiceId,
    string Message,
    DateTimeOffset ScheduledAt) : IRequest<Result<NotificationDto>>;
