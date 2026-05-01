using MediatR;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IEmailVerificationService emailVerificationService)
    : IRequestHandler<ForgotPasswordCommand, Result<EmailVerificationChallengeResult>>
{
    public async Task<Result<EmailVerificationChallengeResult>> Handle(
        ForgotPasswordCommand request,
        CancellationToken ct)
    {
        if (await identityService.IsEmailUniqueAsync(request.Email, ct))
            return new EmailVerificationChallengeResult(request.Email.Trim(), 0);

        return await emailVerificationService.SendPasswordResetOtpAsync(request.Email, ct);
    }
}
