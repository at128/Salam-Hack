using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Customers;
using MediatR;

namespace SalamHack.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    Guid UserId,
    string CustomerName,
    string Email,
    string Phone,
    ClientType ClientType,
    string? CompanyName,
    string? Notes) : IRequest<Result<CustomerDto>>;
