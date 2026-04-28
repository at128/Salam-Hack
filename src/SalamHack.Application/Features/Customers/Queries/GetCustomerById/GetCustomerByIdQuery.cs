using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Customers.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid UserId, Guid CustomerId) : IRequest<Result<CustomerDto>>;
