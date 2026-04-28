namespace SalamHack.Application.Features.Dashboard.Models;

public sealed record DashboardPeriodDto(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc);
