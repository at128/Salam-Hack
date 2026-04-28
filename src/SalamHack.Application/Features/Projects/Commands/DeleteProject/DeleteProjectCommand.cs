using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Projects.Commands.DeleteProject;

public sealed record DeleteProjectCommand(Guid UserId, Guid ProjectId) : IRequest<Result<Deleted>>;
