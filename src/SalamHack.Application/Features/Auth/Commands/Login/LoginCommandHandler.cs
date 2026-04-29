using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Common.Models;
using SalamHack.Application.Settings;
using SalamHack.Contracts.Auth;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Options;

namespace SalamHack.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IRefreshTokenRepository refreshTokenRepository,
    ICookieService cookieService,
    IOptions<RefreshTokenSettings> rtOptions,
    TimeProvider timeProvider

    )
    : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly RefreshTokenSettings _rtSettings = rtOptions.Value;
    public async Task<Result<AuthResponse>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var result = await identityService.ValidateCredentialsAsync(cmd.Email, cmd.Password, ct);
        if (result.IsError)
            return result.TopError;

        var user = result.Value;

        var tokenResult = tokenProvider.GenerateJwtToken(
            new AppUserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Roles));

        if (tokenResult.IsError)        
            return tokenResult.TopError;

        var rawRT = tokenProvider.GenerateRefreshToken(_rtSettings.TokenBytes);
        var rtHash = tokenProvider.HashToken(rawRT);
        var family = Guid.NewGuid().ToString("N");

        var rtData = new RefreshTokenData(
            Id: Guid.CreateVersion7(),
            UserId: user.Id,
            TokenHash: rtHash,
            Family: family,
            IsActive: true,
            IsUsed: false,
            IsRevoked: false,
            ExpiresAt: timeProvider.GetUtcNow().AddDays(_rtSettings.ExpiryDays));

        await refreshTokenRepository.AddAsync(rtData, ct);

        cookieService.SetRefreshTokenCookie(rawRT);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles.FirstOrDefault() ?? "User", 
            user.CreatedAt,
            tokenResult.Value);
    }
}
