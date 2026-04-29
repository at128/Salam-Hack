using FluentValidation;

namespace SalamHack.Application.Features.Analyses.Commands.GenerateProjectAnalysis;

public sealed class GenerateProjectAnalysisCommandValidator : AbstractValidator<GenerateProjectAnalysisCommand>
{
    public GenerateProjectAnalysisCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");
    }
}
