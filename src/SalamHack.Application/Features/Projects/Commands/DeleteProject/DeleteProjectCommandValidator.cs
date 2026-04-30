using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.DeleteProject;

public sealed class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");
    }
}
