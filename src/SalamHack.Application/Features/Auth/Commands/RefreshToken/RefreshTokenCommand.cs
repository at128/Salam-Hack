using SalamHack.Contracts.Auth;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand : IRequest<Result<TokenResponse>>;