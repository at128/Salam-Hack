using FluentValidation;

namespace SalamHack.Application.Features.Projects.Queries.GetProjectHealth;

public sealed class GetProjectHealthQueryValidator : AbstractValidator<GetProjectHealthQuery>
{
    public GetProjectHealthQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");
    }
}
