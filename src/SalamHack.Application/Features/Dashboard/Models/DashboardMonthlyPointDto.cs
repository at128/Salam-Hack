namespace SalamHack.Application.Features.Dashboard.Models;

public sealed record DashboardMonthlyPointDto(
    int Year,
    int Month,
    decimal Revenue,
    decimal Expenses,
    decimal Profit);
