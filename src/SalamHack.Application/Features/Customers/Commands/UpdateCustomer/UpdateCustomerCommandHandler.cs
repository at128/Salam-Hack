using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == cmd.CustomerId && c.UserId == cmd.UserId, ct);

        if (customer is null)
            return ApplicationErrors.Customers.CustomerNotFound;

        var email = cmd.Email.Trim();
        var emailExists = await context.Customers
            .AnyAsync(c => c.UserId == cmd.UserId && c.Id != cmd.CustomerId && c.Email == email, ct);

        if (emailExists)
            return ApplicationErrors.Customers.EmailAlreadyExists;

        var updateResult = customer.Update(
            cmd.CustomerName,
            email,
            cmd.Phone,
            cmd.ClientType,
            cmd.CompanyName,
            cmd.Notes);

        if (updateResult.IsError)
            return updateResult.Errors;

        await context.SaveChangesAsync(ct);

        return customer.ToDto();
    }
}
