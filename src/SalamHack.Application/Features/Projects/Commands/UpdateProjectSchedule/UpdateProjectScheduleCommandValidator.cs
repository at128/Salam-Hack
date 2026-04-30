using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.UpdateProjectSchedule;

public sealed class UpdateProjectScheduleCommandValidator : AbstractValidator<UpdateProjectScheduleCommand>
{
    public UpdateProjectScheduleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("لا يمكن أن يكون تاريخ الانتهاء قبل تاريخ البدء.");
    }
}
