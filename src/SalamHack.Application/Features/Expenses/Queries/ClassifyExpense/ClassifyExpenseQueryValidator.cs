using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Queries.ClassifyExpense;

public sealed class ClassifyExpenseQueryValidator : AbstractValidator<ClassifyExpenseQuery>
{
    public ClassifyExpenseQueryValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("الوصف مطلوب.")
            .MaximumLength(1000);
    }
}
