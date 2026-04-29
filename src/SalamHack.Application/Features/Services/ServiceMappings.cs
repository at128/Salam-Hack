using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Services;

namespace SalamHack.Application.Features.Services;

internal static class ServiceMappings
{
    public static ServiceDto ToDto(this Service service)
        => new(
            service.Id,
            service.ServiceName,
            service.Category,
            service.DefaultHourlyRate,
            service.DefaultRevisions,
            service.IsActive,
            service.CreatedAtUtc,
            service.LastModifiedUtc);
}
