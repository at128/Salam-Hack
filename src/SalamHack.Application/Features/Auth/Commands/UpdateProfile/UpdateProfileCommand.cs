// src/SalamHack.Application/Features/Auth/Commands/UpdateProfile/UpdateProfileCommand.cs
using SalamHack.Contracts.Auth;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    string UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber) : IRequest<Result<ProfileResponse>>;
