using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;
using MediatR;

namespace SalamHack.Application.Features.Projects.Commands.ChangeProjectStatus;

public sealed record ChangeProjectStatusCommand(
    Guid UserId,
    Guid ProjectId,
    ProjectStatus Status) : IRequest<Result<ProjectDto>>;
