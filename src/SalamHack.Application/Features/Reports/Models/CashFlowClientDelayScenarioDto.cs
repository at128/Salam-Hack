namespace SalamHack.Application.Features.Reports.Models;

public sealed record CashFlowClientDelayScenarioDto(
    Guid CustomerId,
    string CustomerName,
    decimal DelayedAmount,
    decimal ForecastBalanceAfterDelay,
    bool WouldGoNegative);
