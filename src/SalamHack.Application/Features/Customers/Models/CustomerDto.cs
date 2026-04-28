using SalamHack.Domain.Customers;

namespace SalamHack.Application.Features.Customers.Models;

public sealed record CustomerDto(
    Guid Id,
    string CustomerName,
    string Email,
    string Phone,
    ClientType ClientType,
    string? CompanyName,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastModifiedUtc);
