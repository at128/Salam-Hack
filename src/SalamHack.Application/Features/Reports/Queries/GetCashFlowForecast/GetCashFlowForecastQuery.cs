using SalamHack.Application.Features.Reports.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Reports.Queries.GetCashFlowForecast;

public sealed record GetCashFlowForecastQuery(
    Guid UserId,
    Guid? DelayedCustomerId = null,
    DateTimeOffset? AsOfUtc = null,
    decimal OpeningBalance = 0,
    DateTimeOffset? OpeningBalanceDateUtc = null) : IRequest<Result<CashFlowForecastDto>>;
