using FluentValidation;

namespace SalamHack.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("معرف العميل مطلوب.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("معرف الخدمة مطلوب.");

        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("اسم المشروع مطلوب.")
            .MaximumLength(200);

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0);

        RuleFor(x => x.ToolCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Revision)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SuggestedPrice)
            .GreaterThan(0);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("لا يمكن أن يكون تاريخ الانتهاء قبل تاريخ البدء.");
    }
}
