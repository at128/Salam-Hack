using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseById;

public sealed class GetExpenseByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetExpenseByIdQuery, Result<ExpenseDto>>
{
    public async Task<Result<ExpenseDto>> Handle(GetExpenseByIdQuery query, CancellationToken ct)
    {
        var expense = await context.Expenses
            .AsNoTracking()
            .Include(e => e.Project)
            .FirstOrDefaultAsync(e => e.Id == query.ExpenseId && e.UserId == query.UserId, ct);

        return expense is null
            ? ApplicationErrors.Expenses.ExpenseNotFound
            : expense.ToDto(expense.Project?.ProjectName);
    }
}
