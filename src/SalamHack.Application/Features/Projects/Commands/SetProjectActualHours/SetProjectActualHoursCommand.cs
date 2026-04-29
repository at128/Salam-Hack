using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Projects.Commands.SetProjectActualHours;

public sealed record SetProjectActualHoursCommand(
    Guid UserId,
    Guid ProjectId,
    decimal ActualHours) : IRequest<Result<ProjectDto>>;
