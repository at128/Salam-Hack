using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.RenameProject;

public sealed class RenameProjectCommandValidator : AbstractValidator<RenameProjectCommand>
{
    public RenameProjectCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");

        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("اسم المشروع مطلوب.")
            .MaximumLength(200);
    }
}
