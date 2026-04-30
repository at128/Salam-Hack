using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.CreateExpense;

public sealed class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("الوصف مطلوب.")
            .MaximumLength(1000);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("العملة مطلوبة.")
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
