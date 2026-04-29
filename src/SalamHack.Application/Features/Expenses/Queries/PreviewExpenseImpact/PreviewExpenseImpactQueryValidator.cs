using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Queries.PreviewExpenseImpact;

public sealed class PreviewExpenseImpactQueryValidator : AbstractValidator<PreviewExpenseImpactQuery>
{
    public PreviewExpenseImpactQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
