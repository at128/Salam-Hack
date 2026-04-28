namespace SalamHack.Application.Features.Dashboard.Models;

public sealed record DashboardSummaryDto(
    DashboardPeriodDto CurrentPeriod,
    decimal MonthlyRevenue,
    decimal MonthlyExpenses,
    decimal MonthlyProfit,
    decimal PendingInvoiceAmount,
    int PendingInvoiceCount,
    int ActiveProjectCount,
    IReadOnlyCollection<DashboardMonthlyPointDto> MonthlyTrend,
    IReadOnlyCollection<DashboardAlertDto> Alerts,
    IReadOnlyCollection<DashboardTransactionDto> RecentTransactions);
