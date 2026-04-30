using FluentValidation;

namespace SalamHack.Application.Features.Services.Queries.GetServiceById;

public sealed class GetServiceByIdQueryValidator : AbstractValidator<GetServiceByIdQuery>
{
    public GetServiceByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("معرف الخدمة مطلوب.");
    }
}
