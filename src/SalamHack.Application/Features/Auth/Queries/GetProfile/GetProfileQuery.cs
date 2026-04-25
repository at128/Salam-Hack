// src/SalamHack.Application/Features/Auth/Queries/GetProfile/GetProfileQuery.cs
using SalamHack.Contracts.Auth;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Auth.Queries.GetProfile;

public sealed record GetProfileQuery(Guid UserId) : IRequest<Result<ProfileResponse>>;
