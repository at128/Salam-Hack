using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IIdentityService identityService,
    IRefreshTokenRepository refreshTokenRepository,
    ICookieService cookieService)
    : IRequestHandler<ChangePasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        ChangePasswordCommand cmd,
        CancellationToken ct)
    {
        var result = await identityService.ChangePasswordAsync(
            cmd.UserId,
            cmd.CurrentPassword,
            cmd.NewPassword,
            ct);

        if (result.IsError)
            return result;

        await refreshTokenRepository.RevokeAllForUserAsync(cmd.UserId, ct);
        cookieService.RemoveRefreshTokenCookie();

        return Result.Success;
    }
}
