using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenses;

public sealed class GetExpensesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetExpensesQuery, Result<PaginatedList<ExpenseListItemDto>>>
{
    public async Task<Result<PaginatedList<ExpenseListItemDto>>> Handle(GetExpensesQuery query, CancellationToken ct)
    {
        var expensesQuery = context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            expensesQuery = expensesQuery.Where(e => e.Description.Contains(search));
        }

        if (query.ProjectId.HasValue)
            expensesQuery = expensesQuery.Where(e => e.ProjectId == query.ProjectId.Value);

        if (query.Category.HasValue)
            expensesQuery = expensesQuery.Where(e => e.Category == query.Category.Value);

        if (query.FromDate.HasValue)
            expensesQuery = expensesQuery.Where(e => e.ExpenseDate >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            expensesQuery = expensesQuery.Where(e => e.ExpenseDate <= query.ToDate.Value);

        if (query.IsRecurring.HasValue)
            expensesQuery = expensesQuery.Where(e => e.IsRecurring == query.IsRecurring.Value);

        var totalCount = await expensesQuery.CountAsync(ct);
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await expensesQuery
            .OrderByDescending(e => e.ExpenseDate)
            .ThenBy(e => e.Description)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExpenseListItemDto(
                e.Id,
                e.ProjectId,
                e.Project == null ? null : e.Project.ProjectName,
                e.Category,
                e.Description,
                e.Amount,
                e.IsRecurring,
                e.ExpenseDate,
                e.Currency))
            .ToListAsync(ct);

        return new PaginatedList<ExpenseListItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }
}
