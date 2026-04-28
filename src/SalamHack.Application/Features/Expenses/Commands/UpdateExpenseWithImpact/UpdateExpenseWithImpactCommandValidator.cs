using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.UpdateExpenseWithImpact;

public sealed class UpdateExpenseWithImpactCommandValidator : AbstractValidator<UpdateExpenseWithImpactCommand>
{
    public UpdateExpenseWithImpactCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("Expense ID is required.");

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(3);
    }
}
