using SalamHack.Domain.Common;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;

namespace SalamHack.Domain.Expenses;

public class Expense : AuditableEntity
{
    private Expense()
    {
    }

    private Expense(
        Guid id,
        Guid userId,
        string description,
        decimal amount,
        ExpenseCategory category,
        DateTimeOffset expenseDate,
        string currency,
        Guid? projectId = null,
        bool isRecurring = false,
        RecurrenceInterval? recurrenceInterval = null,
        DateTimeOffset? recurrenceEndDate = null)
        : base(id)
    {
        UserId = userId;
        ProjectId = projectId;
        Category = category;
        Description = description;
        Amount = amount;
        IsRecurring = isRecurring;
        ExpenseDate = expenseDate;
        RecurrenceInterval = recurrenceInterval;
        RecurrenceEndDate = recurrenceEndDate;
        Currency = currency;
    }

    public Guid UserId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public string Description { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public bool IsRecurring { get; private set; }
    public DateTimeOffset ExpenseDate { get; private set; }
    public RecurrenceInterval? RecurrenceInterval { get; private set; }
    public DateTimeOffset? RecurrenceEndDate { get; private set; }
    public string Currency { get; private set; } = null!;

    public Project? Project { get; private set; }

    public static Result<Expense> Create(
        Guid userId,
        string description,
        decimal amount,
        ExpenseCategory category,
        DateTimeOffset expenseDate,
        string currency,
        Guid? projectId = null,
        bool isRecurring = false,
        RecurrenceInterval? recurrenceInterval = null,
        DateTimeOffset? recurrenceEndDate = null)
    {
        var validation = Validate(
            userId,
            description,
            amount,
            expenseDate,
            currency,
            isRecurring,
            recurrenceInterval,
            recurrenceEndDate);

        if (validation.IsError)
            return validation.Errors;

        NormalizeRecurrence(isRecurring, ref recurrenceInterval, ref recurrenceEndDate);

        return new Expense(
            Guid.CreateVersion7(),
            userId,
            description.Trim(),
            amount,
            category,
            expenseDate,
            currency.Trim(),
            projectId,
            isRecurring,
            recurrenceInterval,
            recurrenceEndDate);
    }

    public Result<Success> Update(
        Guid? projectId,
        ExpenseCategory category,
        string description,
        decimal amount,
        bool isRecurring,
        DateTimeOffset expenseDate,
        RecurrenceInterval? recurrenceInterval,
        DateTimeOffset? recurrenceEndDate,
        string currency)
    {
        var validation = Validate(
            UserId,
            description,
            amount,
            expenseDate,
            currency,
            isRecurring,
            recurrenceInterval,
            recurrenceEndDate);

        if (validation.IsError)
            return validation;

        NormalizeRecurrence(isRecurring, ref recurrenceInterval, ref recurrenceEndDate);

        ProjectId = projectId;
        Category = category;
        Description = description.Trim();
        Amount = amount;
        IsRecurring = isRecurring;
        ExpenseDate = expenseDate;
        RecurrenceInterval = recurrenceInterval;
        RecurrenceEndDate = recurrenceEndDate;
        Currency = currency.Trim();

        return Result.Success;
    }

    private static Result<Success> Validate(
        Guid userId,
        string description,
        decimal amount,
        DateTimeOffset expenseDate,
        string currency,
        bool isRecurring,
        RecurrenceInterval? recurrenceInterval,
        DateTimeOffset? recurrenceEndDate)
    {
        if (userId == Guid.Empty)
            return ExpenseErrors.InvalidUserId;

        if (string.IsNullOrWhiteSpace(description))
            return ExpenseErrors.DescriptionRequired;

        if (amount <= 0)
            return ExpenseErrors.AmountMustBePositive;

        if (string.IsNullOrWhiteSpace(currency))
            return ExpenseErrors.CurrencyRequired;

        if (isRecurring && recurrenceInterval is null)
            return ExpenseErrors.RecurrenceIntervalRequired;

        if (isRecurring && recurrenceEndDate.HasValue && recurrenceEndDate.Value < expenseDate)
            return ExpenseErrors.RecurrenceEndDateBeforeExpenseDate;

        return Result.Success;
    }

    private static void NormalizeRecurrence(
        bool isRecurring,
        ref RecurrenceInterval? recurrenceInterval,
        ref DateTimeOffset? recurrenceEndDate)
    {
        if (isRecurring)
            return;

        recurrenceInterval = null;
        recurrenceEndDate = null;
    }
}
