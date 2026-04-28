using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery query, CancellationToken ct)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .Where(c => c.Id == query.CustomerId && c.UserId == query.UserId)
            .Select(c => new CustomerDto(
                c.Id,
                c.CustomerName,
                c.Email,
                c.Phone,
                c.ClientType,
                c.CompanyName,
                c.Notes,
                c.CreatedAtUtc,
                c.LastModifiedUtc))
            .FirstOrDefaultAsync(ct);

        return customer is null
            ? ApplicationErrors.Customers.CustomerNotFound
            : customer;
    }
}
