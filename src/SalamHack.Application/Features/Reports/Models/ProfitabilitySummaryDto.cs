namespace SalamHack.Application.Features.Reports.Models;

public sealed record ProfitabilitySummaryDto(
    decimal TotalRevenue,
    decimal TotalExpenses,
    decimal TotalProfit,
    decimal OverallMarginPercent);
