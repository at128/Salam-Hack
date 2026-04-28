using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Services;
using MediatR;

namespace SalamHack.Application.Features.Services.Commands.CreateService;

public sealed record CreateServiceCommand(
    Guid UserId,
    string ServiceName,
    ServiceCategory Category,
    decimal DefaultHourlyRate,
    int DefaultRevisions,
    bool IsActive = true) : IRequest<Result<ServiceDto>>;
