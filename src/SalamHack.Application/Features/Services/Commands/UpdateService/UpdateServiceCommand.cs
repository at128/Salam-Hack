using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Services;
using MediatR;

namespace SalamHack.Application.Features.Services.Commands.UpdateService;

public sealed record UpdateServiceCommand(
    Guid UserId,
    Guid ServiceId,
    string ServiceName,
    ServiceCategory Category,
    decimal DefaultHourlyRate,
    int DefaultRevisions) : IRequest<Result<ServiceDto>>;
