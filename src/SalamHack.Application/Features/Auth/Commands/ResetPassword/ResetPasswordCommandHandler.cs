using MediatR;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IEmailVerificationService emailVerificationService,
    IIdentityService identityService)
    : IRequestHandler<ResetPasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        ResetPasswordCommand request,
        CancellationToken ct)
    {
        var verificationResult = await emailVerificationService.VerifyPasswordResetOtpAsync(
            request.Email,
            request.Otp,
            ct);

        if (verificationResult.IsError)
            return verificationResult.TopError;

        return await identityService.ResetPasswordAsync(request.Email, request.NewPassword, ct);
    }
}
