namespace SalamHack.Application.Features.Reports.Models;

public sealed record CashFlowMonthlyPointDto(
    int Year,
    int Month,
    decimal Inflows,
    decimal Outflows,
    decimal NetFlow,
    decimal CumulativeBalance);
