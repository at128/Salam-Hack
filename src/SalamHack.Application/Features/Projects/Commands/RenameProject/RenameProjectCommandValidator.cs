using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.RenameProject;

public sealed class RenameProjectCommandValidator : AbstractValidator<RenameProjectCommand>
{
    public RenameProjectCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200);
    }
}
