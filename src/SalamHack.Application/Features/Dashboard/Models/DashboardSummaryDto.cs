namespace SalamHack.Application.Features.Dashboard.Models;

public sealed record DashboardSummaryDto(
    DashboardPeriodDto CurrentPeriod,
    decimal MonthlyRevenue,
    decimal MonthlyRevenueChangePercent,
    decimal MonthlyExpenses,
    decimal MonthlyExpensesChangePercent,
    decimal MonthlyProfit,
    decimal MonthlyProfitChangePercent,
    decimal PendingInvoiceAmount,
    int PendingInvoiceCount,
    int ActiveProjectCount,
    IReadOnlyCollection<DashboardMonthlyPointDto> MonthlyTrend,
    IReadOnlyCollection<DashboardAlertDto> Alerts,
    IReadOnlyCollection<DashboardTransactionDto> RecentTransactions);
