using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("معرف المصروف مطلوب.");
    }
}
