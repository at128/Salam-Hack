using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Invoices;

public static class InvoiceErrors
{
    public static readonly Error InvalidUserId = Error.Validation(
        "Invoice.InvalidUserId",
        "User id is required.");

    public static readonly Error InvalidProjectId = Error.Validation(
        "Invoice.InvalidProjectId",
        "Project id is required.");

    public static readonly Error InvalidCustomerId = Error.Validation(
        "Invoice.InvalidCustomerId",
        "Customer id is required.");

    public static readonly Error InvoiceNumberRequired = Error.Validation(
        "Invoice.InvoiceNumberRequired",
        "Invoice number is required.");

    public static readonly Error CurrencyRequired = Error.Validation(
        "Invoice.CurrencyRequired",
        "Currency is required.");

    public static readonly Error PaymentCurrencyMismatch = Error.Validation(
        "Invoice.PaymentCurrencyMismatch",
        "Payment currency must match the invoice currency.");

    public static readonly Error TotalAmountMustBePositive = Error.Validation(
        "Invoice.TotalAmountMustBePositive",
        "Invoice total amount must be greater than zero.");

    public static readonly Error AdvanceExceedsTotal = Error.Validation(
        "Invoice.AdvanceExceedsTotal",
        "Advance amount cannot exceed the invoice total including tax.");

    public static readonly Error AdvanceCannotBeNegative = Error.Validation(
        "Invoice.AdvanceCannotBeNegative",
        "Advance amount cannot be negative.");

    public static readonly Error AdvanceNotRequired = Error.Validation(
        "Invoice.AdvanceNotRequired",
        "This invoice does not require an advance payment.");

    public static readonly Error AdvanceAlreadyPaid = Error.Failure(
        "Invoice.AdvanceAlreadyPaid",
        "The required advance payment has already been paid.");

    public static readonly Error DueDateBeforeIssueDate = Error.Validation(
        "Invoice.DueDateBeforeIssueDate",
        "Due date cannot be earlier than issue date.");

    public static readonly Error PaymentAmountMustBePositive = Error.Validation(
        "Invoice.PaymentAmountMustBePositive",
        "Payment amount must be greater than zero.");

    public static readonly Error PaymentExceedsRemainingAmount = Error.Validation(
        "Invoice.PaymentExceedsRemainingAmount",
        "Payment amount exceeds the remaining invoice amount.");

    public static readonly Error CannotPayCancelledInvoice = Error.Failure(
        "Invoice.CannotPayCancelledInvoice",
        "Cannot record a payment against a cancelled invoice.");

    public static readonly Error CannotPayFullyPaidInvoice = Error.Failure(
        "Invoice.CannotPayFullyPaidInvoice",
        "Invoice is already fully paid.");

    public static readonly Error CannotCancelPaidInvoice = Error.Failure(
        "Invoice.CannotCancelPaidInvoice",
        "Cannot cancel a fully paid invoice.");

    public static readonly Error AlreadyCancelled = Error.Failure(
        "Invoice.AlreadyCancelled",
        "Invoice is already cancelled.");

    public static readonly Error OnlyDraftCanBeEdited = Error.Failure(
        "Invoice.OnlyDraftCanBeEdited",
        "Only draft invoices can be edited.");

    public static readonly Error OnlyDraftCanBeSent = Error.Failure(
        "Invoice.OnlyDraftCanBeSent",
        "Only draft invoices can be sent.");

    public static readonly Error OnlyDraftOrPartiallyPaidCanBeSent = Error.Failure(
        "Invoice.OnlyDraftOrPartiallyPaidCanBeSent",
        "Only draft or partially paid invoices can be sent.");

    public static readonly Error CannotSendOverdueInvoice = Error.Failure(
        "Invoice.CannotSendOverdueInvoice",
        "Cannot send an overdue invoice.");

    public static readonly Error CannotMarkPaidInvoiceOverdue = Error.Failure(
        "Invoice.CannotMarkPaidInvoiceOverdue",
        "Cannot mark a paid invoice as overdue.");

    public static readonly Error CannotMarkCancelledInvoiceOverdue = Error.Failure(
        "Invoice.CannotMarkCancelledInvoiceOverdue",
        "Cannot mark a cancelled invoice as overdue.");

    public static readonly Error CannotMarkDraftInvoiceOverdue = Error.Failure(
        "Invoice.CannotMarkDraftInvoiceOverdue",
        "Cannot mark a draft invoice as overdue.");

    public static readonly Error InvoiceNotDue = Error.Validation(
        "Invoice.InvoiceNotDue",
        "Invoice is not overdue yet.");

    public static readonly Error NotFound = Error.NotFound(
        "Invoice.NotFound",
        "Invoice was not found.");
}
