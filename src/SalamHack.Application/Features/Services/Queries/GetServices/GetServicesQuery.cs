using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Services;
using MediatR;

namespace SalamHack.Application.Features.Services.Queries.GetServices;

public sealed record GetServicesQuery(
    Guid UserId,
    string? Search,
    ServiceCategory? Category,
    bool IncludeInactive = false,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<ServiceListItemDto>>>;
