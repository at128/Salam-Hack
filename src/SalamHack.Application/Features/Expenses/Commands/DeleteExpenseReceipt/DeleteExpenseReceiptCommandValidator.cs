using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.DeleteExpenseReceipt;

public sealed class DeleteExpenseReceiptCommandValidator : AbstractValidator<DeleteExpenseReceiptCommand>
{
    public DeleteExpenseReceiptCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("Expense ID is required.");
    }
}
