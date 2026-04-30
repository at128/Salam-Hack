using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.UpdateExpenseWithImpact;

public sealed class UpdateExpenseWithImpactCommandValidator : AbstractValidator<UpdateExpenseWithImpactCommand>
{
    public UpdateExpenseWithImpactCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("معرف المصروف مطلوب.");

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
            .WithMessage("يجب أن تتضمن المصروفات المتكررة فترة التكرار.");

        RuleFor(x => x.RecurrenceEndDate)
            .GreaterThanOrEqualTo(x => x.ExpenseDate)
            .When(x => x.IsRecurring && x.RecurrenceEndDate.HasValue);
    }
}
