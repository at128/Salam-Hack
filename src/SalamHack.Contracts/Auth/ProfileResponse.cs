namespace SalamHack.Contracts.Auth;

public sealed record ProfileResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? BankName,
    string? BankAccountName,
    string? BankIban,
    string Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
