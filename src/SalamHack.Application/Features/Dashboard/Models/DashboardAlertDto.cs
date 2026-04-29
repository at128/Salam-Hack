namespace SalamHack.Application.Features.Dashboard.Models;

public enum DashboardAlertType
{
    Info,
    Success,
    Warning,
    Critical
}

public sealed record DashboardAlertDto(
    DashboardAlertType Type,
    string Message,
    Guid? InvoiceId,
    Guid? ProjectId,
    Guid? CustomerId,
    decimal? Amount,
    DateTimeOffset? DueDate);
