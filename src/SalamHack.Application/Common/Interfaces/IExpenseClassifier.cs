using SalamHack.Domain.Expenses;

namespace SalamHack.Application.Common.Interfaces;

public interface IExpenseClassifier
{
    Task<ExpenseCategory> ClassifyAsync(
        string description,
        CancellationToken cancellationToken = default);
}
