using SalamHack.Application.Common.Interfaces;
using SalamHack.Contracts.Auth;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Queries.GetProfile;

public sealed class GetProfileQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetProfileQuery, Result<ProfileResponse>>
{
    public async Task<Result<ProfileResponse>> Handle(GetProfileQuery query, CancellationToken ct)
    {
        var result = await identityService.GetUserByIdAsync(query.UserId, ct);

        if (result.IsError)
            return result.TopError;

        var user = result.Value;

        return new ProfileResponse(
            user.Id, user.Email, user.FirstName, user.LastName,
            user.PhoneNumber, user.BankName, user.BankAccountName, user.BankIban,
            user.Role, user.CreatedAtUtc, user.UpdatedAtUtc);
    }
}
