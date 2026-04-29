using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Commands.UploadExpenseReceipt;

public sealed class UploadExpenseReceiptCommandHandler(
    IAppDbContext context,
    IExpenseReceiptStorage storage)
    : IRequestHandler<UploadExpenseReceiptCommand, Result<ExpenseReceiptDto>>
{
    public async Task<Result<ExpenseReceiptDto>> Handle(UploadExpenseReceiptCommand cmd, CancellationToken ct)
    {
        var expenseExists = await context.Expenses
            .AsNoTracking()
            .AnyAsync(e => e.Id == cmd.ExpenseId && e.UserId == cmd.UserId, ct);

        if (!expenseExists)
            return ApplicationErrors.Expenses.ExpenseNotFound;

        var file = await storage.SaveAsync(
            cmd.UserId,
            cmd.ExpenseId,
            cmd.FileName,
            cmd.ContentType,
            cmd.Content,
            ct);

        return new ExpenseReceiptDto(
            file.ExpenseId,
            file.FileName,
            file.ContentType,
            file.SizeInBytes,
            file.UploadedAtUtc);
    }
}
