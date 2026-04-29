using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.CreateExpenseWithImpact;

public sealed class CreateExpenseWithImpactCommandValidator : AbstractValidator<CreateExpenseWithImpactCommand>
{
    public CreateExpenseWithImpactCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
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
