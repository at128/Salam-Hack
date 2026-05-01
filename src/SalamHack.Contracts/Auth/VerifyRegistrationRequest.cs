namespace SalamHack.Contracts.Auth;

public sealed record VerifyRegistrationRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Otp);
