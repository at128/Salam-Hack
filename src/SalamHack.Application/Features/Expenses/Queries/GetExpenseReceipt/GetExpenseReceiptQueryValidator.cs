using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseReceipt;

public sealed class GetExpenseReceiptQueryValidator : AbstractValidator<GetExpenseReceiptQuery>
{
    public GetExpenseReceiptQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("Expense ID is required.");
    }
}
