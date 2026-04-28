namespace SalamHack.Application.Features.Reports.Models;

public sealed record CashFlowForecastDto(
    ReportPeriodDto Period,
    CashOpeningBalanceDto OpeningBalance,
    decimal CurrentBalance,
    decimal CurrentMonthInflows,
    decimal CurrentMonthOutflows,
    decimal CurrentMonthNetFlow,
    IReadOnlyCollection<CashFlowMonthlyPointDto> MonthlyTrend,
    CashFlowProjectionDto Projection,
    IReadOnlyCollection<CashFlowPendingInvoiceDto> PendingInvoices,
    IReadOnlyCollection<CashFlowRecurringExpenseDto> RecurringExpenses,
    CashFlowClientDelayScenarioDto? ClientDelayScenario);
