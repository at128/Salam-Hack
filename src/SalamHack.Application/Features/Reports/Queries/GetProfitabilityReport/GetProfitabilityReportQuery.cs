using SalamHack.Application.Features.Reports.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Reports.Queries.GetProfitabilityReport;

public sealed record GetProfitabilityReportQuery(
    Guid UserId,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null) : IRequest<Result<ProfitabilityReportDto>>;
