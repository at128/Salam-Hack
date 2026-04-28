using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Reports.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Expenses;
using SalamHack.Domain.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Reports.Queries.GetCashFlowForecast;

public sealed class GetCashFlowForecastQueryHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<GetCashFlowForecastQuery, Result<CashFlowForecastDto>>
{
    public async Task<Result<CashFlowForecastDto>> Handle(GetCashFlowForecastQuery query, CancellationToken ct)
    {
        var asOfUtc = query.AsOfUtc ?? timeProvider.GetUtcNow();
        var currentMonthStart = new DateTimeOffset(asOfUtc.Year, asOfUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var forecastTo = nextMonthStart.AddMonths(1).AddTicks(-1);
        var trendStart = currentMonthStart.AddMonths(-5);

        var totalInflows = await context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.Project.UserId == query.UserId && p.PaymentDate <= asOfUtc)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        var totalOutflows = await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId && e.ExpenseDate <= asOfUtc)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0;

        var currentMonthInflows = await context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.Project.UserId == query.UserId &&
                        p.PaymentDate >= currentMonthStart &&
                        p.PaymentDate < nextMonthStart)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        var currentMonthOutflows = await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId &&
                        e.ExpenseDate >= currentMonthStart &&
                        e.ExpenseDate < nextMonthStart)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0;

        var pendingInvoices = await context.Invoices
            .AsNoTracking()
            .Where(i => i.Project.UserId == query.UserId &&
                        i.Status != InvoiceStatus.Draft &&
                        i.Status != InvoiceStatus.Paid &&
                        i.Status != InvoiceStatus.Cancelled &&
                        i.TotalWithTax > i.PaidAmount)
            .OrderBy(i => i.DueDate)
            .Select(i => new CashFlowPendingInvoiceDto(
                i.Id,
                i.InvoiceNumber,
                i.CustomerId,
                i.Project.Customer.CustomerName,
                i.TotalWithTax - i.PaidAmount,
                i.DueDate,
                i.DueDate < asOfUtc || i.Status == InvoiceStatus.Overdue,
                i.Currency))
            .ToListAsync(ct);

        var recurringExpenses = await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId &&
                        e.IsRecurring &&
                        e.RecurrenceInterval != null &&
                        (e.RecurrenceEndDate == null || e.RecurrenceEndDate >= asOfUtc))
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new CashFlowRecurringExpenseDto(
                e.Id,
                e.Description,
                e.Amount,
                e.RecurrenceInterval!.Value,
                e.RecurrenceInterval == RecurrenceInterval.Yearly ? Math.Round(e.Amount / 12, 2) : e.Amount,
                e.ExpenseDate,
                e.RecurrenceEndDate,
                e.Currency))
            .ToListAsync(ct);

        var trendPayments = await context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.Project.UserId == query.UserId &&
                        p.PaymentDate >= trendStart &&
                        p.PaymentDate < nextMonthStart)
            .Select(p => new MonthlyAmount(p.PaymentDate.Year, p.PaymentDate.Month, p.Amount))
            .ToListAsync(ct);

        var trendExpenses = await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId &&
                        e.ExpenseDate >= trendStart &&
                        e.ExpenseDate < nextMonthStart)
            .Select(e => new MonthlyAmount(e.ExpenseDate.Year, e.ExpenseDate.Month, e.Amount))
            .ToListAsync(ct);

        var currentBalance = totalInflows - totalOutflows;
        var monthlyTrend = BuildMonthlyTrend(currentMonthStart, trendPayments, trendExpenses, currentBalance);
        var recurringMonthlyOutflow = recurringExpenses.Sum(e => e.MonthlyEquivalentAmount);
        var expectedInflows = pendingInvoices
            .Where(i => i.DueDate <= forecastTo)
            .Sum(i => i.RemainingAmount);
        var expectedOutflows = recurringMonthlyOutflow;
        var expectedNet = expectedInflows - expectedOutflows;
        var forecastBalance = currentBalance + expectedNet;

        var scenario = query.DelayedCustomerId.HasValue
            ? await BuildDelayScenario(query.UserId, query.DelayedCustomerId.Value, forecastBalance, ct)
            : null;

        return new CashFlowForecastDto(
            new ReportPeriodDto(currentMonthStart, nextMonthStart.AddTicks(-1)),
            currentBalance,
            currentMonthInflows,
            currentMonthOutflows,
            currentMonthInflows - currentMonthOutflows,
            monthlyTrend,
            new CashFlowProjectionDto(
                nextMonthStart,
                forecastTo,
                expectedInflows,
                expectedOutflows,
                expectedNet,
                forecastBalance),
            pendingInvoices,
            recurringExpenses,
            scenario);
    }

    private async Task<CashFlowClientDelayScenarioDto?> BuildDelayScenario(
        Guid userId,
        Guid customerId,
        decimal forecastBalance,
        CancellationToken ct)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId && c.UserId == userId, ct);

        if (customer is null)
            return null;

        var delayedAmount = await context.Invoices
            .AsNoTracking()
            .Where(i => i.CustomerId == customerId &&
                        i.Project.UserId == userId &&
                        i.Status != InvoiceStatus.Draft &&
                        i.Status != InvoiceStatus.Paid &&
                        i.Status != InvoiceStatus.Cancelled &&
                        i.TotalWithTax > i.PaidAmount)
            .SumAsync(i => (decimal?)(i.TotalWithTax - i.PaidAmount), ct) ?? 0;

        var balanceAfterDelay = forecastBalance - delayedAmount;

        return new CashFlowClientDelayScenarioDto(
            customer.Id,
            customer.CustomerName,
            delayedAmount,
            balanceAfterDelay,
            balanceAfterDelay < 0);
    }

    private static IReadOnlyCollection<CashFlowMonthlyPointDto> BuildMonthlyTrend(
        DateTimeOffset currentMonthStart,
        IReadOnlyCollection<MonthlyAmount> payments,
        IReadOnlyCollection<MonthlyAmount> expenses,
        decimal currentBalance)
    {
        var points = new List<CashFlowMonthlyPointDto>(capacity: 6);
        var cumulative = currentBalance;
        var monthData = new List<(DateTimeOffset Month, decimal Inflows, decimal Outflows)>();

        for (var offset = -5; offset <= 0; offset++)
        {
            var month = currentMonthStart.AddMonths(offset);
            var inflows = payments
                .Where(p => p.Year == month.Year && p.Month == month.Month)
                .Sum(p => p.Amount);
            var outflows = expenses
                .Where(e => e.Year == month.Year && e.Month == month.Month)
                .Sum(e => e.Amount);

            monthData.Add((month, inflows, outflows));
        }

        cumulative -= monthData.Sum(m => m.Inflows - m.Outflows);

        foreach (var (month, inflows, outflows) in monthData)
        {
            var net = inflows - outflows;
            cumulative += net;
            points.Add(new CashFlowMonthlyPointDto(month.Year, month.Month, inflows, outflows, net, cumulative));
        }

        return points;
    }

    private sealed record MonthlyAmount(int Year, int Month, decimal Amount);
}
