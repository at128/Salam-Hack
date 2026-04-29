using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Customers;
using MediatR;

namespace SalamHack.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery(
    Guid UserId,
    string? Search,
    ClientType? ClientType,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<CustomerListItemDto>>>;
