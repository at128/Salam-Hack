using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.UpdateExpense;

public sealed class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("Expense ID is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(10);

        RuleFor(x => x.RecurrenceInterval)
            .NotNull()
            .When(x => x.IsRecurring)
            .WithMessage("Recurring expenses must include a recurrence interval.");

        RuleFor(x => x.RecurrenceEndDate)
            .GreaterThanOrEqualTo(x => x.ExpenseDate)
            .When(x => x.IsRecurring && x.RecurrenceEndDate.HasValue);
    }
}
