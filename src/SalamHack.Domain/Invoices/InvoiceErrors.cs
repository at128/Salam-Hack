using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Invoices;

public static class InvoiceErrors
{
    public static readonly Error InvalidUserId = Error.Validation(
        "Invoice.InvalidUserId",
        "معرف المستخدم مطلوب.");

    public static readonly Error InvalidProjectId = Error.Validation(
        "Invoice.InvalidProjectId",
        "معرف المشروع مطلوب.");

    public static readonly Error InvalidCustomerId = Error.Validation(
        "Invoice.InvalidCustomerId",
        "معرف العميل مطلوب.");

    public static readonly Error InvoiceNumberRequired = Error.Validation(
        "Invoice.InvoiceNumberRequired",
        "رقم الفاتورة مطلوب.");

    public static readonly Error CurrencyRequired = Error.Validation(
        "Invoice.CurrencyRequired",
        "العملة مطلوبة.");

    public static readonly Error PaymentCurrencyMismatch = Error.Validation(
        "Invoice.PaymentCurrencyMismatch",
        "يجب أن تتطابق عملة الدفع مع عملة الفاتورة.");

    public static readonly Error TotalAmountMustBePositive = Error.Validation(
        "Invoice.TotalAmountMustBePositive",
        "يجب أن يكون المبلغ الإجمالي للفاتورة أكبر من صفر.");

    public static readonly Error AdvanceExceedsTotal = Error.Validation(
        "Invoice.AdvanceExceedsTotal",
        "لا يمكن أن يتجاوز مبلغ الدفعة المقدمة الإجمالي شاملاً الضريبة.");

    public static readonly Error AdvanceCannotBeNegative = Error.Validation(
        "Invoice.AdvanceCannotBeNegative",
        "لا يمكن أن يكون مبلغ الدفعة المقدمة سالباً.");

    public static readonly Error AdvanceNotRequired = Error.Validation(
        "Invoice.AdvanceNotRequired",
        "لا تتطلب هذه الفاتورة دفعة مقدمة.");

    public static readonly Error AdvanceAlreadyPaid = Error.Failure(
        "Invoice.AdvanceAlreadyPaid",
        "تم دفع الدفعة المقدمة المطلوبة بالفعل.");

    public static readonly Error DueDateBeforeIssueDate = Error.Validation(
        "Invoice.DueDateBeforeIssueDate",
        "لا يمكن أن يكون تاريخ الاستحقاق قبل تاريخ الإصدار.");

    public static readonly Error PaymentAmountMustBePositive = Error.Validation(
        "Invoice.PaymentAmountMustBePositive",
        "يجب أن يكون مبلغ الدفع أكبر من صفر.");

    public static readonly Error PaymentExceedsRemainingAmount = Error.Validation(
        "Invoice.PaymentExceedsRemainingAmount",
        "يتجاوز مبلغ الدفع المبلغ المتبقي من الفاتورة.");

    public static readonly Error CannotPayCancelledInvoice = Error.Failure(
        "Invoice.CannotPayCancelledInvoice",
        "لا يمكن تسجيل دفعة لفاتورة ملغاة.");

    public static readonly Error CannotPayFullyPaidInvoice = Error.Failure(
        "Invoice.CannotPayFullyPaidInvoice",
        "تم دفع الفاتورة بالكامل بالفعل.");

    public static readonly Error CannotCancelPaidInvoice = Error.Failure(
        "Invoice.CannotCancelPaidInvoice",
        "لا يمكن إلغاء فاتورة مدفوعة بالكامل.");

    public static readonly Error AlreadyCancelled = Error.Failure(
        "Invoice.AlreadyCancelled",
        "الفاتورة ملغاة بالفعل.");

    public static readonly Error OnlyDraftCanBeEdited = Error.Failure(
        "Invoice.OnlyDraftCanBeEdited",
        "يمكن تعديل الفواتير المسودة فقط.");

    public static readonly Error OnlyDraftCanBeSent = Error.Failure(
        "Invoice.OnlyDraftCanBeSent",
        "يمكن إرسال الفواتير المسودة فقط.");

    public static readonly Error OnlyDraftOrPartiallyPaidCanBeSent = Error.Failure(
        "Invoice.OnlyDraftOrPartiallyPaidCanBeSent",
        "يمكن إرسال الفواتير المسودة أو المدفوعة جزئياً فقط.");

    public static readonly Error CannotSendOverdueInvoice = Error.Failure(
        "Invoice.CannotSendOverdueInvoice",
        "لا يمكن إرسال فاتورة متأخرة.");

    public static readonly Error CannotMarkPaidInvoiceOverdue = Error.Failure(
        "Invoice.CannotMarkPaidInvoiceOverdue",
        "لا يمكن تحديد فاتورة مدفوعة كمتأخرة.");

    public static readonly Error CannotMarkCancelledInvoiceOverdue = Error.Failure(
        "Invoice.CannotMarkCancelledInvoiceOverdue",
        "لا يمكن تحديد فاتورة ملغاة كمتأخرة.");

    public static readonly Error CannotMarkDraftInvoiceOverdue = Error.Failure(
        "Invoice.CannotMarkDraftInvoiceOverdue",
        "لا يمكن تحديد فاتورة مسودة كمتأخرة.");

    public static readonly Error InvoiceNotDue = Error.Validation(
        "Invoice.InvoiceNotDue",
        "الفاتورة لم تتأخر بعد.");

    public static readonly Error OnlyDraftCanBeDeleted = Error.Failure(
        "Invoice.OnlyDraftCanBeDeleted",
        "يمكن حذف الفواتير المسودة فقط. قم بإلغاء الفاتورة بدلاً من ذلك.");

    public static readonly Error NotFound = Error.NotFound(
        "Invoice.NotFound",
        "لم يتم العثور على الفاتورة.");
}
