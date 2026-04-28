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
            .MaximumLength(500);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(3);
    }
}
