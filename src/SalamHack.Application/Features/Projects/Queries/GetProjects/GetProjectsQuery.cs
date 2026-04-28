using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;
using MediatR;

namespace SalamHack.Application.Features.Projects.Queries.GetProjects;

public sealed record GetProjectsQuery(
    Guid UserId,
    string? Search,
    Guid? CustomerId,
    Guid? ServiceId,
    ProjectStatus? Status,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<ProjectListItemDto>>>;
