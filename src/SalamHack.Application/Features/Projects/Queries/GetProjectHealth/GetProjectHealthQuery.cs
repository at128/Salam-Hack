using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Projects.Queries.GetProjectHealth;

public sealed record GetProjectHealthQuery(Guid UserId, Guid ProjectId) : IRequest<Result<ProjectHealthDto>>;
