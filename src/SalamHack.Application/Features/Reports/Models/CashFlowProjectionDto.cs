namespace SalamHack.Application.Features.Reports.Models;

public sealed record CashFlowProjectionDto(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    decimal ExpectedInflows,
    decimal ExpectedOutflows,
    decimal ExpectedNetFlow,
    decimal ForecastBalance);
