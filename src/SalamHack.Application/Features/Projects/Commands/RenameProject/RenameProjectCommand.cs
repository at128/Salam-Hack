using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Projects.Commands.RenameProject;

public sealed record RenameProjectCommand(
    Guid UserId,
    Guid ProjectId,
    string ProjectName) : IRequest<Result<ProjectDto>>;
