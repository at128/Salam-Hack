using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseCategoryBreakdown;

public sealed class GetExpenseCategoryBreakdownQueryValidator : AbstractValidator<GetExpenseCategoryBreakdownQuery>
{
    public GetExpenseCategoryBreakdownQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ToUtc)
            .GreaterThanOrEqualTo(x => x.FromUtc)
            .When(x => x.FromUtc.HasValue && x.ToUtc.HasValue);
    }
}
