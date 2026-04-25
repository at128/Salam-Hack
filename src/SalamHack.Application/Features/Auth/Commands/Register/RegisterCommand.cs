// src/SalamHack.Application/Features/Auth/Commands/Register/RegisterCommand.cs
using SalamHack.Application.Common.Interfaces;
using SalamHack.Contracts.Auth;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber) : IRequest<Result<AuthResponse>>, ISensitiveRequest;
