using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Projects.Queries.GetProjectHealth;

public sealed class GetProjectHealthQueryHandler(IAppDbContext context)
    : IRequestHandler<GetProjectHealthQuery, Result<ProjectHealthDto>>
{
    public async Task<Result<ProjectHealthDto>> Handle(GetProjectHealthQuery query, CancellationToken ct)
    {
        var project = await context.Projects
            .AsNoTracking()
            .Include(p => p.Expenses)
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId && p.UserId == query.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        var health = project.GetHealthSnapshot(project.Expenses.Sum(e => e.Amount));
        if (health.IsError)
            return health.Errors;

        var snapshot = health.Value;
        return new ProjectHealthDto(
            snapshot.BaseCost,
            snapshot.AdditionalExpenses,
            snapshot.TotalCost,
            snapshot.Profit,
            snapshot.MarginPercent,
            snapshot.HourlyProfit,
            snapshot.HealthStatus,
            snapshot.IsHealthy);
    }
}
