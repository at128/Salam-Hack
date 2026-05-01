using MediatR;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Features.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email)
    : IRequest<Result<EmailVerificationChallengeResult>>;
