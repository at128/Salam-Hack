using MediatR;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string Otp,
    string NewPassword) : IRequest<Result<Success>>, ISensitiveRequest;
