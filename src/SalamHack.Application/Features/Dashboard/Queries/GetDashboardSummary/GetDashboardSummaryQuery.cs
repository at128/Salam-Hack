using SalamHack.Application.Features.Dashboard.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(
    Guid UserId,
    DateTimeOffset? AsOfUtc = null,
    int RecentTransactionCount = 6) : IRequest<Result<DashboardSummaryDto>>;
