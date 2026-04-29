using SalamHack.Application.Features.Projects.Commands.ChangeProjectStatus;
using SalamHack.Application.Features.Projects.Commands.CreateProject;
using SalamHack.Application.Features.Projects.Commands.DeleteProject;
using SalamHack.Application.Features.Projects.Commands.RenameProject;
using SalamHack.Application.Features.Projects.Commands.SetProjectActualHours;
using SalamHack.Application.Features.Projects.Commands.UpdateProjectEstimate;
using SalamHack.Application.Features.Projects.Commands.UpdateProjectSchedule;
using SalamHack.Application.Features.Projects.Queries.GetProjectById;
using SalamHack.Application.Features.Projects.Queries.GetProjectHealth;
using SalamHack.Application.Features.Projects.Queries.GetProjects;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class ProjectsController(ISender sender) : ApiController
{
    [HttpGet]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetProjects(
        [FromQuery] string? search,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? serviceId,
        [FromQuery] ProjectStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetProjectsQuery(userId, search, customerId, serviceId, status, pageNumber, pageSize),
            ct);

        return result.Match(projects => OkResponse(projects), Problem);
    }

    [HttpGet("{projectId:guid}")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetProject(Guid projectId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetProjectByIdQuery(userId, projectId), ct);

        return result.Match(project => OkResponse(project), Problem);
    }

    [HttpGet("{projectId:guid}/health")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetProjectHealth(Guid projectId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetProjectHealthQuery(userId, projectId), ct);

        return result.Match(health => OkResponse(health), Problem);
    }

    [HttpPost]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateProject(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CreateProjectCommand(
            userId,
            request.CustomerId,
            request.ServiceId,
            request.ProjectName,
            request.EstimatedHours,
            request.ToolCost,
            request.Revision,
            request.IsUrgent,
            request.SuggestedPrice,
            request.StartDate,
            request.EndDate), ct);

        return result.Match(
            project => CreatedResponse(nameof(GetProject), new { projectId = project.Id }, project, "Project created successfully."),
            Problem);
    }

    [HttpPatch("{projectId:guid}/status")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> ChangeStatus(
        Guid projectId,
        [FromBody] ChangeProjectStatusRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new ChangeProjectStatusCommand(userId, projectId, request.Status),
            ct);

        return result.Match(project => OkResponse(project, "Project status updated successfully."), Problem);
    }

    [HttpPatch("{projectId:guid}/estimate")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> UpdateEstimate(
        Guid projectId,
        [FromBody] UpdateProjectEstimateRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new UpdateProjectEstimateCommand(
            userId,
            projectId,
            request.EstimatedHours,
            request.ToolCost,
            request.Revision,
            request.IsUrgent,
            request.SuggestedPrice), ct);

        return result.Match(project => OkResponse(project, "Project estimate updated successfully."), Problem);
    }

    [HttpPatch("{projectId:guid}/schedule")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> UpdateSchedule(
        Guid projectId,
        [FromBody] UpdateProjectScheduleRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new UpdateProjectScheduleCommand(userId, projectId, request.StartDate, request.EndDate),
            ct);

        return result.Match(project => OkResponse(project, "Project schedule updated successfully."), Problem);
    }

    [HttpPatch("{projectId:guid}/actual-hours")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> SetActualHours(
        Guid projectId,
        [FromBody] SetProjectActualHoursRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new SetProjectActualHoursCommand(userId, projectId, request.ActualHours),
            ct);

        return result.Match(project => OkResponse(project, "Project actual hours updated successfully."), Problem);
    }

    [HttpPatch("{projectId:guid}/name")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> RenameProject(
        Guid projectId,
        [FromBody] RenameProjectRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new RenameProjectCommand(userId, projectId, request.ProjectName),
            ct);

        return result.Match(project => OkResponse(project, "Project renamed successfully."), Problem);
    }

    [HttpDelete("{projectId:guid}")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> DeleteProject(Guid projectId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new DeleteProjectCommand(userId, projectId), ct);

        return result.Match(_ => DeletedResponse("Project deleted successfully."), Problem);
    }
}

public sealed record CreateProjectRequest(
    Guid CustomerId,
    Guid ServiceId,
    string ProjectName,
    decimal EstimatedHours,
    decimal ToolCost,
    int Revision,
    bool IsUrgent,
    decimal SuggestedPrice,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);

public sealed record ChangeProjectStatusRequest(ProjectStatus Status);

public sealed record UpdateProjectEstimateRequest(
    decimal EstimatedHours,
    decimal ToolCost,
    int Revision,
    bool IsUrgent,
    decimal SuggestedPrice);

public sealed record UpdateProjectScheduleRequest(
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);

public sealed record SetProjectActualHoursRequest(decimal ActualHours);

public sealed record RenameProjectRequest(string ProjectName);
