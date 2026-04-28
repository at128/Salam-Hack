using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Services.Queries.GetServiceById;

public sealed record GetServiceByIdQuery(Guid UserId, Guid ServiceId) : IRequest<Result<ServiceDto>>;
