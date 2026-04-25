using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<Result<Success>>, ISensitiveRequest;
