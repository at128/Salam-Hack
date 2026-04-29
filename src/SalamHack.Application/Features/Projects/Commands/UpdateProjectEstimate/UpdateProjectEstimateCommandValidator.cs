using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.UpdateProjectEstimate;

public sealed class UpdateProjectEstimateCommandValidator : AbstractValidator<UpdateProjectEstimateCommand>
{
    public UpdateProjectEstimateCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0);

        RuleFor(x => x.ToolCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Revision)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SuggestedPrice)
            .GreaterThan(0);
    }
}
