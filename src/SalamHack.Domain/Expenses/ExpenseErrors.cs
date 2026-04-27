using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Expenses;

public static class ExpenseErrors
{
    public static readonly Error InvalidUserId = Error.Validation(
        "Expense.InvalidUserId",
        "User id is required.");

    public static readonly Error DescriptionRequired = Error.Validation(
        "Expense.DescriptionRequired",
        "Description is required.");

    public static readonly Error AmountMustBePositive = Error.Validation(
        "Expense.AmountMustBePositive",
        "Expense amount must be greater than zero.");

    public static readonly Error CurrencyRequired = Error.Validation(
        "Expense.CurrencyRequired",
        "Currency is required.");

    public static readonly Error RecurrenceIntervalRequired = Error.Validation(
        "Expense.RecurrenceIntervalRequired",
        "Recurring expenses must include a recurrence interval.");

    public static readonly Error RecurrenceEndDateBeforeExpenseDate = Error.Validation(
        "Expense.RecurrenceEndDateBeforeExpenseDate",
        "Recurrence end date cannot be earlier than the expense date.");
}
