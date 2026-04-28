using SalamHack.Domain.Common;

namespace SalamHack.Domain.Projects.Events;

public sealed class ProjectStatusChangedDomainEvent(
    Guid projectId,
    Guid customerId,
    ProjectStatus oldStatus,
    ProjectStatus newStatus) : DomainEvent
{
    public Guid ProjectId { get; } = projectId;
    public Guid CustomerId { get; } = customerId;
    public ProjectStatus OldStatus { get; } = oldStatus;
    public ProjectStatus NewStatus { get; } = newStatus;
}
