using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseCategoryBreakdown;

public sealed class GetExpenseCategoryBreakdownQueryHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<GetExpenseCategoryBreakdownQuery, Result<ExpenseCategoryBreakdownDto>>
{
    public async Task<Result<ExpenseCategoryBreakdownDto>> Handle(GetExpenseCategoryBreakdownQuery query, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();
        var defaultTo = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(1).AddTicks(-1);
        var toUtc = query.ToUtc ?? defaultTo;
        var fromUtc = query.FromUtc ?? new DateTimeOffset(toUtc.Year, toUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var expensesQuery = context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId &&
                        e.ExpenseDate >= fromUtc &&
                        e.ExpenseDate <= toUtc);

        if (query.ProjectId.HasValue)
            expensesQuery = expensesQuery.Where(e => e.ProjectId == query.ProjectId.Value);

        var groupings = await expensesQuery
            .GroupBy(e => e.Category)
            .Select(g => new
            {
                Category = g.Key,
                TotalAmount = g.Sum(e => e.Amount),
                ExpenseCount = g.Count()
            })
            .ToListAsync(ct);

        var totalAmount = groupings.Sum(g => g.TotalAmount);
        var totalCount = groupings.Sum(g => g.ExpenseCount);

        var items = groupings
            .OrderByDescending(g => g.TotalAmount)
            .Select(g => new ExpenseCategoryBreakdownItemDto(
                g.Category,
                g.TotalAmount,
                g.ExpenseCount,
                totalAmount > 0
                    ? Math.Round(g.TotalAmount / totalAmount * 100, 2)
                    : 0))
            .ToList();

        return new ExpenseCategoryBreakdownDto(
            fromUtc,
            toUtc,
            totalAmount,
            totalCount,
            items);
    }
}
