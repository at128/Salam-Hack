using FluentValidation;

namespace SalamHack.Application.Features.Services.Commands.DeleteService;

public sealed class DeleteServiceCommandValidator : AbstractValidator<DeleteServiceCommand>
{
    public DeleteServiceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("معرف الخدمة مطلوب.");
    }
}
