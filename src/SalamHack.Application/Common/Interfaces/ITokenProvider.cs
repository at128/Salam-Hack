using System.Security.Claims;
using SalamHack.Application.Common.Models;
using SalamHack.Contracts.Auth;
using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Common.Interfaces;

public interface ITokenProvider
{
    Result<TokenResponse> GenerateJwtToken(AppUserDto user);

    string GenerateRefreshToken(int tokenBytes = 64);

    string HashToken(string token);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
