using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Queries.ClassifyExpense;

public sealed class ClassifyExpenseQueryHandler(IExpenseClassifier expenseClassifier)
    : IRequestHandler<ClassifyExpenseQuery, Result<ExpenseClassificationDto>>
{
    public async Task<Result<ExpenseClassificationDto>> Handle(ClassifyExpenseQuery query, CancellationToken ct)
    {
        var category = await expenseClassifier.ClassifyAsync(query.Description, ct);
        return new ExpenseClassificationDto(category);
    }
}
