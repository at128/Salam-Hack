namespace SalamHack.Application.Features.Reports.Models;

public sealed record ReportPeriodDto(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc);
