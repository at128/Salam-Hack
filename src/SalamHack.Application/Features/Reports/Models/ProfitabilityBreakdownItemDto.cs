namespace SalamHack.Application.Features.Reports.Models;

public enum ProfitabilityBreakdownType
{
    Service,
    Customer,
    Project
}

public sealed record ProfitabilityBreakdownItemDto(
    Guid? EntityId,
    string Name,
    ProfitabilityBreakdownType Type,
    decimal Revenue,
    decimal Cost,
    decimal Profit,
    decimal MarginPercent);
