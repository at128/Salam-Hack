namespace SalamHack.Application.Features.Reports.Models;

public sealed record ProfitabilityMonthlyPointDto(
    int Year,
    int Month,
    decimal Revenue,
    decimal Expenses,
    decimal Profit);
