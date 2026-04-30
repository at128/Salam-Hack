using FluentValidation;

namespace SalamHack.Application.Features.Analyses.Commands.MarkAnalysisReviewed;

public sealed class MarkAnalysisReviewedCommandValidator : AbstractValidator<MarkAnalysisReviewedCommand>
{
    public MarkAnalysisReviewedCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.AnalysisId)
            .NotEmpty().WithMessage("معرف التحليل مطلوب.");
    }
}
