using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Customers.Queries.GetCustomerProfile;

public sealed record GetCustomerProfileQuery(
    Guid UserId,
    Guid CustomerId) : IRequest<Result<CustomerProfileDto>>;
