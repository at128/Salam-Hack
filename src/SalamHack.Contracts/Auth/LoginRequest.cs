namespace SalamHack.Contracts.Auth;

public sealed record LoginRequest(string Email,
                                  string Password);
