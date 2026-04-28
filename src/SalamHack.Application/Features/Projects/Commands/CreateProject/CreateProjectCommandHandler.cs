using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand cmd, CancellationToken ct)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == cmd.CustomerId && c.UserId == cmd.UserId, ct);

        if (customer is null)
            return ApplicationErrors.Customers.CustomerNotFound;

        var service = await context.Services
            .FirstOrDefaultAsync(s => s.Id == cmd.ServiceId && s.UserId == cmd.UserId, ct);

        if (service is null)
            return ApplicationErrors.Services.ServiceNotFound;

        if (!service.IsActive)
            return ApplicationErrors.Services.InactiveServiceCannotBeUsed;

        var projectName = cmd.ProjectName.Trim();
        var nameExists = await context.Projects
            .AnyAsync(p => p.UserId == cmd.UserId && p.ProjectName == projectName, ct);

        if (nameExists)
            return ApplicationErrors.Projects.ProjectNameAlreadyExists;

        var projectResult = Project.Create(
            cmd.UserId,
            cmd.CustomerId,
            cmd.ServiceId,
            projectName,
            cmd.EstimatedHours,
            cmd.ToolCost,
            cmd.Revision,
            cmd.IsUrgent,
            cmd.SuggestedPrice,
            cmd.StartDate,
            cmd.EndDate);

        if (projectResult.IsError)
            return projectResult.Errors;

        var project = projectResult.Value;
        context.Projects.Add(project);
        await context.SaveChangesAsync(ct);

        return project.ToDto(
            customer.CustomerName,
            service.ServiceName,
            service.Category,
            additionalExpenses: 0);
    }
}
