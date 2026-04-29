using SalamHack.Domain.Services;

namespace SalamHack.Application.Features.Services.Models;

public sealed record ServiceListItemDto(
    Guid Id,
    string ServiceName,
    ServiceCategory Category,
    decimal DefaultHourlyRate,
    int DefaultRevisions,
    bool IsActive,
    int ProjectsCount,
    DateTimeOffset CreatedAtUtc);
