using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(
    Guid UserId,
    Guid CustomerId,
    Guid ServiceId,
    string ProjectName,
    decimal EstimatedHours,
    decimal ToolCost,
    int Revision,
    bool IsUrgent,
    decimal SuggestedPrice,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate) : IRequest<Result<ProjectDto>>;
