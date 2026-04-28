using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceEntity = SalamHack.Domain.Services.Service;

namespace SalamHack.Application.Features.Services.Commands.CreateService;

public sealed class CreateServiceCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateServiceCommand, Result<ServiceDto>>
{
    public async Task<Result<ServiceDto>> Handle(CreateServiceCommand cmd, CancellationToken ct)
    {
        var serviceName = cmd.ServiceName.Trim();

        var nameExists = await context.Services
            .AnyAsync(s => s.UserId == cmd.UserId && s.ServiceName == serviceName, ct);

        if (nameExists)
            return ApplicationErrors.Services.ServiceNameAlreadyExists;

        var serviceResult = ServiceEntity.Create(
            cmd.UserId,
            serviceName,
            cmd.Category,
            cmd.DefaultHourlyRate,
            cmd.DefaultRevisions,
            cmd.IsActive);

        if (serviceResult.IsError)
            return serviceResult.Errors;

        var service = serviceResult.Value;
        context.Services.Add(service);
        await context.SaveChangesAsync(ct);

        return service.ToDto();
    }
}
