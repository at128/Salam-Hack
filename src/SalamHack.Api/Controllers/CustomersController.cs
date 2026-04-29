using SalamHack.Application.Features.Customers.Commands.CreateCustomer;
using SalamHack.Application.Features.Customers.Commands.DeleteCustomer;
using SalamHack.Application.Features.Customers.Commands.UpdateCustomer;
using SalamHack.Application.Features.Customers.Queries.GetCustomerById;
using SalamHack.Application.Features.Customers.Queries.GetCustomerProfile;
using SalamHack.Application.Features.Customers.Queries.GetCustomers;
using SalamHack.Domain.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class CustomersController(ISender sender) : ApiController
{
    [HttpGet]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? search,
        [FromQuery] ClientType? clientType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetCustomersQuery(userId, search, clientType, pageNumber, pageSize),
            ct);

        return result.Match(customers => OkResponse(customers), Problem);
    }

    [HttpGet("{customerId:guid}")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetCustomer(Guid customerId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetCustomerByIdQuery(userId, customerId), ct);

        return result.Match(customer => OkResponse(customer), Problem);
    }

    [HttpGet("{customerId:guid}/profile")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetCustomerProfile(Guid customerId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetCustomerProfileQuery(userId, customerId), ct);

        return result.Match(profile => OkResponse(profile), Problem);
    }

    [HttpPost]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CustomerRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CreateCustomerCommand(
            userId,
            request.CustomerName,
            request.Email,
            request.Phone,
            request.ClientType,
            request.CompanyName,
            request.Notes), ct);

        return result.Match(
            customer => CreatedResponse(nameof(GetCustomer), new { customerId = customer.Id }, customer, "Customer created successfully."),
            Problem);
    }

    [HttpPut("{customerId:guid}")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> UpdateCustomer(
        Guid customerId,
        [FromBody] CustomerRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new UpdateCustomerCommand(
            userId,
            customerId,
            request.CustomerName,
            request.Email,
            request.Phone,
            request.ClientType,
            request.CompanyName,
            request.Notes), ct);

        return result.Match(customer => OkResponse(customer, "Customer updated successfully."), Problem);
    }

    [HttpDelete("{customerId:guid}")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> DeleteCustomer(Guid customerId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new DeleteCustomerCommand(userId, customerId), ct);

        return result.Match(_ => DeletedResponse("Customer deleted successfully."), Problem);
    }
}

public sealed record CustomerRequest(
    string CustomerName,
    string Email,
    string Phone,
    ClientType ClientType,
    string? CompanyName,
    string? Notes);
