
using SalamHack.Application.Common.Interfaces;
using SalamHack.Contracts.Auth;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponse>> , ISensitiveRequest;
