using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Notifications.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Notifications.Commands.CreateInvoiceReminder;

public sealed class CreateInvoiceReminderCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateInvoiceReminderCommand, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(CreateInvoiceReminderCommand cmd, CancellationToken ct)
    {
        var invoiceExists = await context.Invoices
            .AsNoTracking()
            .AnyAsync(i => i.Id == cmd.InvoiceId && i.Project.UserId == cmd.UserId, ct);

        if (!invoiceExists)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        var notification = Notification.CreateReminder(
            cmd.UserId,
            cmd.InvoiceId,
            cmd.Message,
            cmd.ScheduledAt);

        context.Notifications.Add(notification);
        await context.SaveChangesAsync(ct);

        return notification.ToDto();
    }
}
