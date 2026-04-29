using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseReceipt;

public sealed class GetExpenseReceiptQueryHandler(
    IAppDbContext context,
    IExpenseReceiptStorage storage)
    : IRequestHandler<GetExpenseReceiptQuery, Result<ExpenseReceiptFileDto>>
{
    public async Task<Result<ExpenseReceiptFileDto>> Handle(GetExpenseReceiptQuery query, CancellationToken ct)
    {
        var expenseExists = await context.Expenses
            .AsNoTracking()
            .AnyAsync(e => e.Id == query.ExpenseId && e.UserId == query.UserId, ct);

        if (!expenseExists)
            return ApplicationErrors.Expenses.ExpenseNotFound;

        var file = await storage.GetAsync(query.UserId, query.ExpenseId, ct);
        if (file is null)
            return ApplicationErrors.Expenses.ExpenseReceiptNotFound;

        return new ExpenseReceiptFileDto(
            file.ExpenseId,
            file.FileName,
            file.ContentType,
            file.SizeInBytes,
            file.UploadedAtUtc,
            file.Content);
    }
}
