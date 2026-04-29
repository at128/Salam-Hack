using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Customers.Commands.DeleteCustomer;

public sealed class DeleteCustomerCommandHandler(IAppDbContext context, TimeProvider timeProvider)
    : IRequestHandler<DeleteCustomerCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == cmd.CustomerId && c.UserId == cmd.UserId, ct);

        if (customer is null)
            return ApplicationErrors.Customers.CustomerNotFound;

        customer.Delete(timeProvider.GetUtcNow());
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
