using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Services.Queries.GetServiceById;

public sealed class GetServiceByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetServiceByIdQuery, Result<ServiceDto>>
{
    public async Task<Result<ServiceDto>> Handle(GetServiceByIdQuery query, CancellationToken ct)
    {
        var service = await context.Services
            .AsNoTracking()
            .Where(s => s.Id == query.ServiceId && s.UserId == query.UserId)
            .Select(s => new ServiceDto(
                s.Id,
                s.ServiceName,
                s.Category,
                s.DefaultHourlyRate,
                s.DefaultRevisions,
                s.IsActive,
                s.CreatedAtUtc,
                s.LastModifiedUtc))
            .FirstOrDefaultAsync(ct);

        return service is null
            ? ApplicationErrors.Services.ServiceNotFound
            : service;
    }
}
