using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.SetProjectActualHours;

public sealed class SetProjectActualHoursCommandValidator : AbstractValidator<SetProjectActualHoursCommand>
{
    public SetProjectActualHoursCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");

        RuleFor(x => x.ActualHours)
            .GreaterThanOrEqualTo(0);
    }
}
