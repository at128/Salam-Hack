namespace SalamHack.Application.Features.Reports.Models;

public sealed record CashOpeningBalanceDto(
    decimal Amount,
    DateTimeOffset? EffectiveAtUtc);
