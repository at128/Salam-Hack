using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Customers;
using MediatR;

namespace SalamHack.Application.Features.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid UserId,
    Guid CustomerId,
    string CustomerName,
    string Email,
    string Phone,
    ClientType ClientType,
    string? CompanyName,
    string? Notes) : IRequest<Result<CustomerDto>>;
