namespace SalamHack.Domain.Notifications;

public enum NotificationType
{
    PaymentReminder,
    OverduePaymentAlert,
    PaymentReceived,
    InvoiceCancelled,
    ProjectStatusChanged,
    ProjectHealthWarning,
    ExpenseSpike
}
