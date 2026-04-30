using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.DeleteExpenseReceipt;

public sealed class DeleteExpenseReceiptCommandValidator : AbstractValidator<DeleteExpenseReceiptCommand>
{
    public DeleteExpenseReceiptCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("معرف المصروف مطلوب.");
    }
}
