using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Projects.Commands.UpdateProjectSchedule;

public sealed record UpdateProjectScheduleCommand(
    Guid UserId,
    Guid ProjectId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate) : IRequest<Result<ProjectDto>>;
