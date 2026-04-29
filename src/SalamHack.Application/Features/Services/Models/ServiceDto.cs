using SalamHack.Domain.Services;

namespace SalamHack.Application.Features.Services.Models;

public sealed record ServiceDto(
    Guid Id,
    string ServiceName,
    ServiceCategory Category,
    decimal DefaultHourlyRate,
    int DefaultRevisions,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastModifiedUtc);
