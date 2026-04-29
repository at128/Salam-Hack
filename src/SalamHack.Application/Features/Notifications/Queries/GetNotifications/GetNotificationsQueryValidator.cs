using FluentValidation;

namespace SalamHack.Application.Features.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 100);
    }
}
