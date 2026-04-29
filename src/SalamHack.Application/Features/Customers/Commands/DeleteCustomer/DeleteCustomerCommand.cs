using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Customers.Commands.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid UserId, Guid CustomerId) : IRequest<Result<Deleted>>;
