using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Commands.DeleteExpenseReceipt;

public sealed class DeleteExpenseReceiptCommandHandler(
    IAppDbContext context,
    IExpenseReceiptStorage storage)
    : IRequestHandler<DeleteExpenseReceiptCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteExpenseReceiptCommand cmd, CancellationToken ct)
    {
        var expenseExists = await context.Expenses
            .AsNoTracking()
            .AnyAsync(e => e.Id == cmd.ExpenseId && e.UserId == cmd.UserId, ct);

        if (!expenseExists)
            return ApplicationErrors.Expenses.ExpenseNotFound;

        var deleted = await storage.DeleteAsync(cmd.UserId, cmd.ExpenseId, ct);

        return deleted
            ? Result.Deleted
            : ApplicationErrors.Expenses.ExpenseReceiptNotFound;
    }
}
