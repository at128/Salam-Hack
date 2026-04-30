using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseReceipt;

public sealed class GetExpenseReceiptQueryValidator : AbstractValidator<GetExpenseReceiptQuery>
{
    public GetExpenseReceiptQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("معرف المصروف مطلوب.");
    }
}
