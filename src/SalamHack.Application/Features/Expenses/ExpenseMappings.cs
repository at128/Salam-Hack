using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Expenses;

namespace SalamHack.Application.Features.Expenses;

internal static class ExpenseMappings
{
    public static ExpenseDto ToDto(this Expense expense, string? projectName)
        => new(
            expense.Id,
            expense.ProjectId,
            projectName,
            expense.Category,
            expense.Description,
            expense.Amount,
            expense.IsRecurring,
            expense.ExpenseDate,
            expense.RecurrenceInterval,
            expense.RecurrenceEndDate,
            expense.Currency,
            expense.CreatedAtUtc,
            expense.LastModifiedUtc);
}
