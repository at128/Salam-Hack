using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Services.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Services.Commands.SetServiceActiveStatus;

public sealed class SetServiceActiveStatusCommandHandler(IAppDbContext context)
    : IRequestHandler<SetServiceActiveStatusCommand, Result<ServiceDto>>
{
    public async Task<Result<ServiceDto>> Handle(SetServiceActiveStatusCommand cmd, CancellationToken ct)
    {
        var service = await context.Services
            .FirstOrDefaultAsync(s => s.Id == cmd.ServiceId && s.UserId == cmd.UserId, ct);

        if (service is null)
            return ApplicationErrors.Services.ServiceNotFound;

        if (cmd.IsActive)
            service.Activate();
        else
            service.Deactivate();

        await context.SaveChangesAsync(ct);

        return service.ToDto();
    }
}
