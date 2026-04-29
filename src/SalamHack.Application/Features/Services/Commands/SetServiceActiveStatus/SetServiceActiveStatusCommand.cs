using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Services.Commands.SetServiceActiveStatus;

public sealed record SetServiceActiveStatusCommand(
    Guid UserId,
    Guid ServiceId,
    bool IsActive) : IRequest<Result<ServiceDto>>;
