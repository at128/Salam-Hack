using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Reports.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Reports.Queries.GetProfitabilityReport;

public sealed class GetProfitabilityReportQueryHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<GetProfitabilityReportQuery, Result<ProfitabilityReportDto>>
{
    public async Task<Result<ProfitabilityReportDto>> Handle(GetProfitabilityReportQuery query, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();
        var defaultTo = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(1).AddTicks(-1);
        var toUtc = query.ToUtc ?? defaultTo;
        var fromUtc = query.FromUtc ?? new DateTimeOffset(toUtc.Year, toUtc.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(-5);

        var projects = await context.Projects
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Service)
            .Include(p => p.Expenses)
            .Where(p => p.UserId == query.UserId &&
                        p.Status == ProjectStatus.Completed &&
                        p.EndDate >= fromUtc &&
                        p.EndDate <= toUtc)
            .ToListAsync(ct);

        var projectRows = projects
            .Select(p =>
            {
                var health = p.GetHealthSnapshot(p.Expenses.Sum(e => e.Amount));
                var cost = health.IsError ? 0 : health.Value.TotalCost;

                return new ProjectProfitRow(
                    p.Id,
                    p.ProjectName,
                    p.CustomerId,
                    p.Customer.CustomerName,
                    p.ServiceId,
                    p.Service.ServiceName,
                    p.EndDate,
                    p.SuggestedPrice,
                    cost);
            })
            .ToList();

        var totalRevenue = projectRows.Sum(p => p.Revenue);
        var totalExpenses = projectRows.Sum(p => p.Cost);
        var totalProfit = totalRevenue - totalExpenses;
        var summary = new ProfitabilitySummaryDto(
            totalRevenue,
            totalExpenses,
            totalProfit,
            CalculateMargin(totalRevenue, totalExpenses));

        var byService = projectRows
            .GroupBy(p => new { p.ServiceId, p.ServiceName })
            .Select(g => ToBreakdown(g.Key.ServiceId, g.Key.ServiceName, ProfitabilityBreakdownType.Service, g))
            .OrderByDescending(i => i.MarginPercent)
            .ToList();

        var byCustomer = projectRows
            .GroupBy(p => new { p.CustomerId, p.CustomerName })
            .Select(g => ToBreakdown(g.Key.CustomerId, g.Key.CustomerName, ProfitabilityBreakdownType.Customer, g))
            .OrderByDescending(i => i.MarginPercent)
            .ToList();

        var byProject = projectRows
            .Select(p => ToBreakdown(p.ProjectId, p.ProjectName, ProfitabilityBreakdownType.Project, [p]))
            .OrderByDescending(i => i.MarginPercent)
            .ToList();

        return new ProfitabilityReportDto(
            new ReportPeriodDto(fromUtc, toUtc),
            summary,
            BuildMonthlyTrend(fromUtc, toUtc, projectRows),
            byService,
            byCustomer,
            byProject,
            byProject.Take(3).ToList(),
            byProject.OrderBy(i => i.MarginPercent).Take(3).ToList(),
            BuildInsight(byService));
    }

    private static IReadOnlyCollection<ProfitabilityMonthlyPointDto> BuildMonthlyTrend(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        IReadOnlyCollection<ProjectProfitRow> rows)
    {
        var start = new DateTimeOffset(fromUtc.Year, fromUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(toUtc.Year, toUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var trend = new List<ProfitabilityMonthlyPointDto>();

        for (var month = start; month <= end; month = month.AddMonths(1))
        {
            var revenue = rows
                .Where(r => r.RecognizedAt.Year == month.Year && r.RecognizedAt.Month == month.Month)
                .Sum(r => r.Revenue);
            var expenses = rows
                .Where(r => r.RecognizedAt.Year == month.Year && r.RecognizedAt.Month == month.Month)
                .Sum(r => r.Cost);

            trend.Add(new ProfitabilityMonthlyPointDto(
                month.Year,
                month.Month,
                revenue,
                expenses,
                revenue - expenses));
        }

        return trend;
    }

    private static ProfitabilityBreakdownItemDto ToBreakdown(
        Guid? entityId,
        string name,
        ProfitabilityBreakdownType type,
        IEnumerable<ProjectProfitRow> rows)
    {
        var materialized = rows.ToList();
        var revenue = materialized.Sum(p => p.Revenue);
        var cost = materialized.Sum(p => p.Cost);

        return new ProfitabilityBreakdownItemDto(
            entityId,
            name,
            type,
            revenue,
            cost,
            revenue - cost,
            CalculateMargin(revenue, cost));
    }

    private static string BuildInsight(IReadOnlyCollection<ProfitabilityBreakdownItemDto> byService)
    {
        if (byService.Count == 0)
            return "No completed project profitability data exists for the selected period.";

        var best = byService.OrderByDescending(s => s.MarginPercent).First();
        var weakest = byService.OrderBy(s => s.MarginPercent).First();

        if (best.EntityId == weakest.EntityId)
            return $"{best.Name} is the only service with profitability data. Its margin is {best.MarginPercent}%.";

        return $"{weakest.Name} has the lowest service margin at {weakest.MarginPercent}%, while {best.Name} leads at {best.MarginPercent}%.";
    }

    private static decimal CalculateMargin(decimal revenue, decimal cost)
        => revenue > 0 ? Math.Round((revenue - cost) / revenue * 100, 2) : 0;

    private sealed record ProjectProfitRow(
        Guid ProjectId,
        string ProjectName,
        Guid CustomerId,
        string CustomerName,
        Guid ServiceId,
        string ServiceName,
        DateTimeOffset RecognizedAt,
        decimal Revenue,
        decimal Cost);
}
