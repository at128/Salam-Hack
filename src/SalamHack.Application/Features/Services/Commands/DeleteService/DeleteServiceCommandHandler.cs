using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Services.Commands.DeleteService;

public sealed class DeleteServiceCommandHandler(IAppDbContext context, TimeProvider timeProvider)
    : IRequestHandler<DeleteServiceCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteServiceCommand cmd, CancellationToken ct)
    {
        var service = await context.Services
            .FirstOrDefaultAsync(s => s.Id == cmd.ServiceId && s.UserId == cmd.UserId, ct);

        if (service is null)
            return ApplicationErrors.Services.ServiceNotFound;

        service.Delete(timeProvider.GetUtcNow());
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
