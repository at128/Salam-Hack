using FluentValidation;

namespace SalamHack.Application.Features.Projects.Queries.GetProjectHealth;

public sealed class GetProjectHealthQueryValidator : AbstractValidator<GetProjectHealthQuery>
{
    public GetProjectHealthQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");
    }
}
