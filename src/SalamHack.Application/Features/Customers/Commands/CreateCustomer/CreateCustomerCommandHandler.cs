using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim();

        var emailExists = await context.Customers
            .AnyAsync(c => c.UserId == cmd.UserId && c.Email == email, ct);

        if (emailExists)
            return ApplicationErrors.Customers.EmailAlreadyExists;

        var customerResult = Customer.Create(
            cmd.UserId,
            cmd.CustomerName,
            email,
            cmd.Phone,
            cmd.ClientType,
            cmd.CompanyName,
            cmd.Notes);

        if (customerResult.IsError)
            return customerResult.Errors;

        var customer = customerResult.Value;
        context.Customers.Add(customer);
        await context.SaveChangesAsync(ct);

        return customer.ToDto();
    }
}
