using FluentValidation;

namespace SalamHack.Application.Features.Analyses.Commands.GenerateProjectAnalysis;

public sealed class GenerateProjectAnalysisCommandValidator : AbstractValidator<GenerateProjectAnalysisCommand>
{
    public GenerateProjectAnalysisCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");
    }
}
