using MediatR;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Contracts.Auth;
using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Features.Auth.Commands.VerifyRegistration;

public sealed record VerifyRegistrationCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Otp) : IRequest<Result<AuthResponse>>, ISensitiveRequest;
