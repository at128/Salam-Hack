using FluentValidation;

namespace SalamHack.Application.Features.Analyses.Commands.MarkAnalysisReviewed;

public sealed class MarkAnalysisReviewedCommandValidator : AbstractValidator<MarkAnalysisReviewedCommand>
{
    public MarkAnalysisReviewedCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.AnalysisId)
            .NotEmpty().WithMessage("Analysis ID is required.");
    }
}
