using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Commands.LogoutAllSessions;

public sealed record LogoutAllSessionsCommand(Guid UserId)
    : IRequest<Result<Success>>;