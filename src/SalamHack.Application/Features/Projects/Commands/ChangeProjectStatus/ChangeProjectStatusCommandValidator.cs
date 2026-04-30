using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.ChangeProjectStatus;

public sealed class ChangeProjectStatusCommandValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
