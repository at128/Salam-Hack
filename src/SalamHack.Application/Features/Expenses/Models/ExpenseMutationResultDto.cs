namespace SalamHack.Application.Features.Expenses.Models;

public sealed record ExpenseMutationResultDto(
    ExpenseDto Expense,
    IReadOnlyCollection<ExpenseChangeImpactDto> Impacts);
