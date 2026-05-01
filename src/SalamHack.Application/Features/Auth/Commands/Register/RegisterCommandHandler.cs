using MediatR;
using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IIdentityService identityService,
    IEmailVerificationService emailVerificationService) : IRequestHandler<RegisterCommand, Result<EmailVerificationChallengeResult>>
{
    public async Task<Result<EmailVerificationChallengeResult>> Handle(
        RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        if (!await identityService.IsEmailUniqueAsync(email, ct))
            return ApplicationErrors.Auth.EmailAlreadyRegistered;

        return await emailVerificationService.SendRegistrationOtpAsync(email, ct);
    }
}
