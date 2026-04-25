using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Commands.LogoutCurrentSession;

public sealed record LogoutCurrentSessionCommand(Guid UserId)
    : IRequest<Result<Success>>;