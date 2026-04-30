using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseById;

public sealed class GetExpenseByIdQueryValidator : AbstractValidator<GetExpenseByIdQuery>
{
    public GetExpenseByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("معرف المصروف مطلوب.");
    }
}
