using SalamHack.Application.Common.DomainEvents;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Notifications;
using SalamHack.Domain.Projects.Events;
using MediatR;

namespace SalamHack.Application.Features.Projects.EventHandlers;

public sealed class ProjectStatusChangedDomainEventHandler(IAppDbContext context)
    : INotificationHandler<ProjectStatusChangedDomainEvent>
{
    public async Task Handle(ProjectStatusChangedDomainEvent notification, CancellationToken ct)
    {
        var userId = await DomainEventHandlerHelpers.GetProjectOwnerIdAsync(
            context,
            notification.ProjectId,
            ct);

        if (userId is null)
            return;

        var message = $"Project status changed from {notification.OldStatus} to {notification.NewStatus}.";

        var statusNotification = Notification.CreateProjectStatusChanged(
            userId.Value,
            notification.ProjectId,
            message);

        await DomainEventHandlerHelpers.AddNotificationIfMissingAsync(context, statusNotification, ct);
        await context.SaveChangesAsync(ct);
    }
}
