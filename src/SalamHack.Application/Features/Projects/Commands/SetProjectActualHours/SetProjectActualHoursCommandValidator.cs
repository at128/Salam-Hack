using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.SetProjectActualHours;

public sealed class SetProjectActualHoursCommandValidator : AbstractValidator<SetProjectActualHoursCommand>
{
    public SetProjectActualHoursCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.ActualHours)
            .GreaterThanOrEqualTo(0);
    }
}
