// src/SalamHack.Application/Features/Auth/Commands/LogoutAllSessions/LogoutAllSessionsCommandHandler.cs
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Commands.LogoutAllSessions;

public sealed class LogoutAllSessionsCommandHandler(
    IRefreshTokenRepository refreshTokenRepo,
    ICookieService cookieService)           
    : IRequestHandler<LogoutAllSessionsCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        LogoutAllSessionsCommand cmd, CancellationToken ct)
    {
        await refreshTokenRepo.RevokeAllForUserAsync(cmd.UserId, ct);
        cookieService.RemoveRefreshTokenCookie();

        return Result.Success;
    }
}