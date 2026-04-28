using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("Expense ID is required.");
    }
}
