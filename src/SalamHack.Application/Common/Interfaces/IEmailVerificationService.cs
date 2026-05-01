using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Common.Interfaces;

public interface IEmailVerificationService
{
    Task<Result<EmailVerificationChallengeResult>> SendRegistrationOtpAsync(
        string email,
        CancellationToken ct = default);

    Task<Result<Success>> VerifyRegistrationOtpAsync(
        string email,
        string otp,
        CancellationToken ct = default);

    Task<Result<EmailVerificationChallengeResult>> SendPasswordResetOtpAsync(
        string email,
        CancellationToken ct = default);

    Task<Result<Success>> VerifyPasswordResetOtpAsync(
        string email,
        string otp,
        CancellationToken ct = default);
}

public sealed record EmailVerificationChallengeResult(
    string Email,
    int ExpiresInMinutes);
