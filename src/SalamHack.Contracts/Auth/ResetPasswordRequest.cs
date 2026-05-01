namespace SalamHack.Contracts.Auth;

public sealed record ResetPasswordRequest(
    string Email,
    string Otp,
    string NewPassword);
