using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Projects.Commands.UpdateProjectEstimate;

public sealed record UpdateProjectEstimateCommand(
    Guid UserId,
    Guid ProjectId,
    decimal EstimatedHours,
    decimal ToolCost,
    int Revision,
    bool IsUrgent,
    decimal SuggestedPrice) : IRequest<Result<ProjectDto>>;
