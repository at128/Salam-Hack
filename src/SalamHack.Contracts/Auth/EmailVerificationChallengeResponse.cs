namespace SalamHack.Contracts.Auth;

public sealed record EmailVerificationChallengeResponse(
    string Email,
    int ExpiresInMinutes);
