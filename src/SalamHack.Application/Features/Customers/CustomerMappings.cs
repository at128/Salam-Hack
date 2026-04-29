using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Customers;

namespace SalamHack.Application.Features.Customers;

internal static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer customer)
        => new(
            customer.Id,
            customer.CustomerName,
            customer.Email,
            customer.Phone,
            customer.ClientType,
            customer.CompanyName,
            customer.Notes,
            customer.CreatedAtUtc,
            customer.LastModifiedUtc);

    public static CustomerListItemDto ToListItemDto(this Customer customer)
        => new(
            customer.Id,
            customer.CustomerName,
            customer.Email,
            customer.Phone,
            customer.ClientType,
            customer.CompanyName,
            customer.Projects.Count,
            customer.CreatedAtUtc);
}
