using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Queries.PreviewExpenseImpact;

public sealed class PreviewExpenseImpactQueryValidator : AbstractValidator<PreviewExpenseImpactQuery>
{
    public PreviewExpenseImpactQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
