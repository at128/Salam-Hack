using SalamHack.Domain.Customers;

namespace SalamHack.Application.Features.Customers.Models;

public sealed record CustomerListItemDto(
    Guid Id,
    string CustomerName,
    string Email,
    string Phone,
    ClientType ClientType,
    string? CompanyName,
    int ProjectsCount,
    DateTimeOffset CreatedAtUtc);
