namespace SalamHack.Contracts.Auth;

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? BankName,
    string? BankAccountName,
    string? BankIban);
