using FluentValidation;

namespace SalamHack.Application.Features.Analyses.Queries.GetProjectAnalyses;

public sealed class GetProjectAnalysesQueryValidator : AbstractValidator<GetProjectAnalysesQuery>
{
    public GetProjectAnalysesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue);
    }
}
