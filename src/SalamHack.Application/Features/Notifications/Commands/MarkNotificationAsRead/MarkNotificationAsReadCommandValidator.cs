using FluentValidation;

namespace SalamHack.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.NotificationId)
            .NotEmpty().WithMessage("معرف الإشعار مطلوب.");
    }
}
