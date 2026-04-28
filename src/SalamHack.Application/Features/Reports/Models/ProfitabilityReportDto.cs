namespace SalamHack.Application.Features.Reports.Models;

public sealed record ProfitabilityReportDto(
    ReportPeriodDto Period,
    ProfitabilitySummaryDto Summary,
    IReadOnlyCollection<ProfitabilityMonthlyPointDto> MonthlyTrend,
    IReadOnlyCollection<ProfitabilityBreakdownItemDto> ByService,
    IReadOnlyCollection<ProfitabilityBreakdownItemDto> ByCustomer,
    IReadOnlyCollection<ProfitabilityBreakdownItemDto> ByProject,
    IReadOnlyCollection<ProfitabilityBreakdownItemDto> TopPerformers,
    IReadOnlyCollection<ProfitabilityBreakdownItemDto> LowestPerformers,
    string Insight);
