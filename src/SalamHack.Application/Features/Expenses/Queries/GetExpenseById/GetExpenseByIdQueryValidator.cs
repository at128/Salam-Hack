using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseById;

public sealed class GetExpenseByIdQueryValidator : AbstractValidator<GetExpenseByIdQuery>
{
    public GetExpenseByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("Expense ID is required.");
    }
}
