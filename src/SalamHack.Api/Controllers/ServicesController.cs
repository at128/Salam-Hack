using SalamHack.Application.Features.Services.Commands.CreateService;
using SalamHack.Application.Features.Services.Commands.DeleteService;
using SalamHack.Application.Features.Services.Commands.SetServiceActiveStatus;
using SalamHack.Application.Features.Services.Commands.UpdateService;
using SalamHack.Application.Features.Services.Queries.GetServiceById;
using SalamHack.Application.Features.Services.Queries.GetServices;
using SalamHack.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class ServicesController(ISender sender) : ApiController
{
    [HttpGet]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetServices(
        [FromQuery] string? search,
        [FromQuery] ServiceCategory? category,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetServicesQuery(userId, search, category, includeInactive, pageNumber, pageSize),
            ct);

        return result.Match(services => OkResponse(services), Problem);
    }

    [HttpGet("{serviceId:guid}")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetService(Guid serviceId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetServiceByIdQuery(userId, serviceId), ct);

        return result.Match(service => OkResponse(service), Problem);
    }

    [HttpPost]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateService(
        [FromBody] CreateServiceRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CreateServiceCommand(
            userId,
            request.ServiceName,
            request.Category,
            request.DefaultHourlyRate,
            request.DefaultRevisions,
            request.IsActive), ct);

        return result.Match(
            service => CreatedResponse(nameof(GetService), new { serviceId = service.Id }, service, "Service created successfully."),
            Problem);
    }

    [HttpPut("{serviceId:guid}")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> UpdateService(
        Guid serviceId,
        [FromBody] UpdateServiceRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new UpdateServiceCommand(
            userId,
            serviceId,
            request.ServiceName,
            request.Category,
            request.DefaultHourlyRate,
            request.DefaultRevisions), ct);

        return result.Match(service => OkResponse(service, "Service updated successfully."), Problem);
    }

    [HttpPatch("{serviceId:guid}/active")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> SetActiveStatus(
        Guid serviceId,
        [FromBody] SetServiceActiveRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new SetServiceActiveStatusCommand(userId, serviceId, request.IsActive),
            ct);

        return result.Match(service => OkResponse(service, "Service status updated successfully."), Problem);
    }

    [HttpDelete("{serviceId:guid}")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> DeleteService(Guid serviceId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new DeleteServiceCommand(userId, serviceId), ct);

        return result.Match(_ => DeletedResponse("Service deleted successfully."), Problem);
    }
}

public sealed record CreateServiceRequest(
    string ServiceName,
    ServiceCategory Category,
    decimal DefaultHourlyRate,
    int DefaultRevisions,
    bool IsActive = true);

public sealed record UpdateServiceRequest(
    string ServiceName,
    ServiceCategory Category,
    decimal DefaultHourlyRate,
    int DefaultRevisions);

public sealed record SetServiceActiveRequest(bool IsActive);
