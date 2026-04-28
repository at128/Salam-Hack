using FluentValidation;

namespace SalamHack.Application.Features.Notifications.Commands.SendDueNotifications;

public sealed class SendDueNotificationsCommandValidator : AbstractValidator<SendDueNotificationsCommand>
{
    public SendDueNotificationsCommandValidator()
    {
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 500);
    }
}
