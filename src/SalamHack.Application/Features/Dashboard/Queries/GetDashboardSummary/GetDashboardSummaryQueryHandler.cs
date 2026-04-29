using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Dashboard.Models;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery query, CancellationToken ct)
    {
        var asOfUtc = query.AsOfUtc ?? timeProvider.GetUtcNow();
        var monthStart = new DateTimeOffset(asOfUtc.Year, asOfUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var nextMonthStart = monthStart.AddMonths(1);
        var previousMonthStart = monthStart.AddMonths(-1);
        var trendStart = monthStart.AddMonths(-5);

        var monthlyRevenue = await context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.UserId == query.UserId &&
                        p.PaymentDate >= monthStart &&
                        p.PaymentDate < nextMonthStart)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        var monthlyExpenses = await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId &&
                        e.ExpenseDate >= monthStart &&
                        e.ExpenseDate < nextMonthStart)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0;

        var previousMonthRevenue = await context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.UserId == query.UserId &&
                        p.PaymentDate >= previousMonthStart &&
                        p.PaymentDate < monthStart)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        var previousMonthExpenses = await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId &&
                        e.ExpenseDate >= previousMonthStart &&
                        e.ExpenseDate < monthStart)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0;

        var monthlyProfit = monthlyRevenue - monthlyExpenses;
        var previousMonthProfit = previousMonthRevenue - previousMonthExpenses;

        var pendingInvoicesQuery = context.Invoices
            .AsNoTracking()
            .Where(i => i.UserId == query.UserId &&
                        i.Status != InvoiceStatus.Draft &&
                        i.Status != InvoiceStatus.Paid &&
                        i.Status != InvoiceStatus.Cancelled &&
                        i.TotalWithTax > i.PaidAmount);

        var pendingInvoiceAmount = await pendingInvoicesQuery
            .SumAsync(i => (decimal?)(i.TotalWithTax - i.PaidAmount), ct) ?? 0;

        var pendingInvoiceCount = await pendingInvoicesQuery.CountAsync(ct);

        var activeProjectCount = await context.Projects
            .AsNoTracking()
            .CountAsync(p => p.UserId == query.UserId &&
                             (p.Status == ProjectStatus.Planning || p.Status == ProjectStatus.InProgress), ct);

        var paymentPoints = await context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.UserId == query.UserId &&
                        p.PaymentDate >= trendStart &&
                        p.PaymentDate < nextMonthStart)
            .Select(p => new MonthlyAmount(p.PaymentDate.Year, p.PaymentDate.Month, p.Amount))
            .ToListAsync(ct);

        var expensePoints = await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId &&
                        e.ExpenseDate >= trendStart &&
                        e.ExpenseDate < nextMonthStart)
            .Select(e => new MonthlyAmount(e.ExpenseDate.Year, e.ExpenseDate.Month, e.Amount))
            .ToListAsync(ct);

        var trend = BuildTrend(monthStart, paymentPoints, expensePoints);
        var recentTransactionCount = Math.Clamp(query.RecentTransactionCount, 1, 20);
        var recentTransactions = await GetRecentTransactions(query.UserId, recentTransactionCount, ct);
        var alerts = await GetAlerts(query.UserId, asOfUtc, ct);

        return new DashboardSummaryDto(
            new DashboardPeriodDto(monthStart, nextMonthStart.AddTicks(-1)),
            monthlyRevenue,
            CalculateChangePercent(monthlyRevenue, previousMonthRevenue),
            monthlyExpenses,
            CalculateChangePercent(monthlyExpenses, previousMonthExpenses),
            monthlyProfit,
            CalculateChangePercent(monthlyProfit, previousMonthProfit),
            pendingInvoiceAmount,
            pendingInvoiceCount,
            activeProjectCount,
            trend,
            alerts,
            recentTransactions);
    }

    private async Task<IReadOnlyCollection<DashboardTransactionDto>> GetRecentTransactions(
        Guid userId,
        int take,
        CancellationToken ct)
    {
        var payments = await context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.UserId == userId)
            .OrderByDescending(p => p.PaymentDate)
            .Take(take)
            .Select(p => new DashboardTransactionDto(
                p.PaymentDate,
                $"Payment for invoice {p.Invoice.InvoiceNumber}",
                p.Amount,
                DashboardTransactionType.Payment,
                p.InvoiceId,
                null,
                p.Invoice.CustomerId,
                p.Currency))
            .ToListAsync(ct);

        var expenses = await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.ExpenseDate)
            .Take(take)
            .Select(e => new DashboardTransactionDto(
                e.ExpenseDate,
                e.Description,
                -e.Amount,
                DashboardTransactionType.Expense,
                null,
                e.Id,
                null,
                e.Currency))
            .ToListAsync(ct);

        return payments
            .Concat(expenses)
            .OrderByDescending(t => t.Date)
            .Take(take)
            .ToList();
    }

    private async Task<IReadOnlyCollection<DashboardAlertDto>> GetAlerts(
        Guid userId,
        DateTimeOffset asOfUtc,
        CancellationToken ct)
    {
        var overdueInvoices = await context.Invoices
            .AsNoTracking()
            .Where(i => i.UserId == userId &&
                        i.Status != InvoiceStatus.Draft &&
                        i.Status != InvoiceStatus.Paid &&
                        i.Status != InvoiceStatus.Cancelled &&
                        i.TotalWithTax > i.PaidAmount &&
                        (i.Status == InvoiceStatus.Overdue || i.DueDate < asOfUtc))
            .OrderBy(i => i.DueDate)
            .Take(3)
            .Select(i => new DashboardAlertDto(
                DashboardAlertType.Warning,
                $"Invoice {i.InvoiceNumber} is overdue.",
                i.Id,
                i.ProjectId,
                i.CustomerId,
                i.TotalWithTax - i.PaidAmount,
                i.DueDate))
            .ToListAsync(ct);

        var activeProjects = await context.Projects
            .AsNoTracking()
            .Include(p => p.Expenses)
            .Where(p => p.UserId == userId &&
                        (p.Status == ProjectStatus.Planning || p.Status == ProjectStatus.InProgress))
            .ToListAsync(ct);

        var projectAlerts = activeProjects
            .Select(p => new
            {
                Project = p,
                Health = p.GetHealthSnapshot(p.Expenses.Sum(e => e.Amount))
            })
            .Where(x => !x.Health.IsError &&
                        x.Health.Value.MarginPercent < ApplicationConstants.BusinessRules.HealthyMarginThreshold)
            .OrderBy(x => x.Health.Value.MarginPercent)
            .Take(3)
            .Select(x => new DashboardAlertDto(
                x.Health.Value.MarginPercent < ApplicationConstants.BusinessRules.AtRiskMarginThreshold
                    ? DashboardAlertType.Critical
                    : DashboardAlertType.Warning,
                $"Project {x.Project.ProjectName} margin is {x.Health.Value.MarginPercent}%.",
                null,
                x.Project.Id,
                x.Project.CustomerId,
                x.Health.Value.Profit,
                x.Project.EndDate))
            .ToList();

        var alerts = overdueInvoices.Concat(projectAlerts).ToList();

        if (alerts.Count == 0)
        {
            alerts.Add(new DashboardAlertDto(
                DashboardAlertType.Success,
                "No urgent finance alerts.",
                null,
                null,
                null,
                null,
                null));
        }

        return alerts;
    }

    private static IReadOnlyCollection<DashboardMonthlyPointDto> BuildTrend(
        DateTimeOffset monthStart,
        IReadOnlyCollection<MonthlyAmount> payments,
        IReadOnlyCollection<MonthlyAmount> expenses)
    {
        var points = new List<DashboardMonthlyPointDto>(capacity: 6);

        for (var offset = -5; offset <= 0; offset++)
        {
            var month = monthStart.AddMonths(offset);
            var revenue = payments
                .Where(p => p.Year == month.Year && p.Month == month.Month)
                .Sum(p => p.Amount);
            var expense = expenses
                .Where(e => e.Year == month.Year && e.Month == month.Month)
                .Sum(e => e.Amount);

            points.Add(new DashboardMonthlyPointDto(
                month.Year,
                month.Month,
                revenue,
                expense,
                revenue - expense));
        }

        return points;
    }

    private static decimal CalculateChangePercent(decimal current, decimal previous)
        => previous == 0
            ? current == 0 ? 0 : 100
            : Math.Round((current - previous) / Math.Abs(previous) * 100, 2);

    private sealed record MonthlyAmount(int Year, int Month, decimal Amount);
}
