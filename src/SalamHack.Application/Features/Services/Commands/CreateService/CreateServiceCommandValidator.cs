using FluentValidation;

namespace SalamHack.Application.Features.Services.Commands.CreateService;

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("اسم الخدمة مطلوب.")
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.DefaultHourlyRate)
            .GreaterThan(0);

        RuleFor(x => x.DefaultRevisions)
            .GreaterThanOrEqualTo(0);
    }
}
