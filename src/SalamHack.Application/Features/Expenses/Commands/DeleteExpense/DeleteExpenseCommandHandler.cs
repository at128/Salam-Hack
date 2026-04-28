using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteExpenseCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteExpenseCommand cmd, CancellationToken ct)
    {
        var expense = await context.Expenses
            .FirstOrDefaultAsync(e => e.Id == cmd.ExpenseId && e.UserId == cmd.UserId, ct);

        if (expense is null)
            return ApplicationErrors.Expenses.ExpenseNotFound;

        context.Expenses.Remove(expense);
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
