using FluentValidation;

namespace SalamHack.Application.Features.Analyses.Commands.SaveAnalysis;

public sealed class SaveAnalysisCommandValidator : AbstractValidator<SaveAnalysisCommand>
{
    public SaveAnalysisCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.WhatHappened)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.WhatItMeans)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.WhatToDo)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.HealthStatus)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Title)
            .MaximumLength(200);

        RuleFor(x => x.Summary)
            .MaximumLength(1000);

        RuleFor(x => x.ConfidenceScore)
            .InclusiveBetween(0, 1)
            .When(x => x.ConfidenceScore.HasValue);
    }
}
