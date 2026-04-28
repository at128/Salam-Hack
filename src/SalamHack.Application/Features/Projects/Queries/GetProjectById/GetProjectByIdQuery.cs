using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Projects.Queries.GetProjectById;

public sealed record GetProjectByIdQuery(Guid UserId, Guid ProjectId) : IRequest<Result<ProjectDto>>;
