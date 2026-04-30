using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Expenses;

public static class ExpenseErrors
{
    public static readonly Error InvalidUserId = Error.Validation(
        "Expense.InvalidUserId",
        "معرف المستخدم مطلوب.");

    public static readonly Error DescriptionRequired = Error.Validation(
        "Expense.DescriptionRequired",
        "الوصف مطلوب.");

    public static readonly Error AmountMustBePositive = Error.Validation(
        "Expense.AmountMustBePositive",
        "يجب أن يكون مبلغ المصروف أكبر من صفر.");

    public static readonly Error CurrencyRequired = Error.Validation(
        "Expense.CurrencyRequired",
        "العملة مطلوبة.");

    public static readonly Error RecurrenceIntervalRequired = Error.Validation(
        "Expense.RecurrenceIntervalRequired",
        "يجب أن تتضمن المصروفات المتكررة فترة التكرار.");

    public static readonly Error RecurrenceEndDateBeforeExpenseDate = Error.Validation(
        "Expense.RecurrenceEndDateBeforeExpenseDate",
        "لا يمكن أن يكون تاريخ نهاية التكرار قبل تاريخ المصروف.");
}
