using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Services.Commands.UpdateService;

public sealed class UpdateServiceCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateServiceCommand, Result<ServiceDto>>
{
    public async Task<Result<ServiceDto>> Handle(UpdateServiceCommand cmd, CancellationToken ct)
    {
        var service = await context.Services
            .FirstOrDefaultAsync(s => s.Id == cmd.ServiceId && s.UserId == cmd.UserId, ct);

        if (service is null)
            return ApplicationErrors.Services.ServiceNotFound;

        var serviceName = cmd.ServiceName.Trim();
        var nameExists = await context.Services
            .AnyAsync(s => s.UserId == cmd.UserId && s.Id != cmd.ServiceId && s.ServiceName == serviceName, ct);

        if (nameExists)
            return ApplicationErrors.Services.ServiceNameAlreadyExists;

        var updateResult = service.Update(
            serviceName,
            cmd.Category,
            cmd.DefaultHourlyRate,
            cmd.DefaultRevisions);

        if (updateResult.IsError)
            return updateResult.Errors;

        await context.SaveChangesAsync(ct);

        return service.ToDto();
    }
}
