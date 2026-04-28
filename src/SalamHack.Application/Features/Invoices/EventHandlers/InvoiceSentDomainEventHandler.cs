using SalamHack.Application.Common.DomainEvents;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Invoices.Events;
using SalamHack.Domain.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.EventHandlers;

public sealed class InvoiceSentDomainEventHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : INotificationHandler<InvoiceSentDomainEvent>
{
    public async Task Handle(InvoiceSentDomainEvent notification, CancellationToken ct)
    {
        var userId = await DomainEventHandlerHelpers.GetProjectOwnerIdAsync(
            context,
            notification.ProjectId,
            ct);

        if (userId is null)
            return;

        var invoiceNumber = await context.Invoices
            .Where(i => i.Id == notification.InvoiceId)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        var dueDate = DomainEventHandlerHelpers.FormatDate(notification.DueDate);
        var message = $"Invoice {invoiceNumber ?? notification.InvoiceId.ToString()} was sent and is due on {dueDate}.";
        var reminderAt = GetReminderDate(notification.DueDate);

        var reminder = Notification.CreateReminder(
            userId.Value,
            notification.InvoiceId,
            message,
            reminderAt);

        await DomainEventHandlerHelpers.AddNotificationIfMissingAsync(context, reminder, ct);
        await context.SaveChangesAsync(ct);
    }

    private DateTimeOffset GetReminderDate(DateTimeOffset dueDate)
    {
        var now = timeProvider.GetUtcNow();
        var preferredReminderDate = dueDate.AddDays(-3);

        if (preferredReminderDate > now)
            return preferredReminderDate;

        return dueDate > now ? dueDate : now;
    }
}
