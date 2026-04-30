using FluentValidation;

namespace SalamHack.Application.Features.Analyses.Queries.GetProjectAnalyses;

public sealed class GetProjectAnalysesQueryValidator : AbstractValidator<GetProjectAnalysesQuery>
{
    public GetProjectAnalysesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue);
    }
}
