using FluentValidation;

namespace SalamHack.Application.Features.Analyses.Queries.GetProjectAnalysisDashboard;

public sealed class GetProjectAnalysisDashboardQueryValidator : AbstractValidator<GetProjectAnalysisDashboardQuery>
{
    public GetProjectAnalysisDashboardQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");
    }
}
