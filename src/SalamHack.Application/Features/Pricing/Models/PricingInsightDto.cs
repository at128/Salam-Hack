namespace SalamHack.Application.Features.Pricing.Models;

public enum PricingInsightSeverity
{
    Info,
    Success,
    Warning,
    Critical
}

public sealed record PricingInsightDto(
    PricingInsightSeverity Severity,
    string Message);
