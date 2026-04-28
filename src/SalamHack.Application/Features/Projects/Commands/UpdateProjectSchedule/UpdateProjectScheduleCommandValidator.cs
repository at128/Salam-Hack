using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.UpdateProjectSchedule;

public sealed class UpdateProjectScheduleCommandValidator : AbstractValidator<UpdateProjectScheduleCommand>
{
    public UpdateProjectScheduleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date cannot be earlier than start date.");
    }
}
