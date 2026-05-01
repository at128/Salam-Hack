using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Common.Errors;

public static class ApplicationErrors
{
    public static class Customers
    {
        public static readonly Error CustomerNotFound =
            Error.NotFound("Customers.CustomerNotFound", "لم يتم العثور على العميل.");

        public static readonly Error EmailAlreadyExists =
            Error.Conflict("Customers.EmailAlreadyExists", "يوجد عميل بنفس البريد الإلكتروني بالفعل.");
    }

    public static class Services
    {
        public static readonly Error ServiceNotFound =
            Error.NotFound("Services.ServiceNotFound", "لم يتم العثور على الخدمة.");

        public static readonly Error ServiceNameAlreadyExists =
            Error.Conflict("Services.ServiceNameAlreadyExists", "توجد خدمة بنفس الاسم بالفعل.");

        public static readonly Error InactiveServiceCannotBeUsed =
            Error.Conflict("Services.InactiveServiceCannotBeUsed", "لا يمكن استخدام خدمة غير نشطة لمشروع جديد.");
    }

    public static class Projects
    {
        public static readonly Error ProjectNotFound =
            Error.NotFound("Projects.ProjectNotFound", "لم يتم العثور على المشروع.");

        public static readonly Error ProjectNameAlreadyExists =
            Error.Conflict("Projects.ProjectNameAlreadyExists", "يوجد مشروع بنفس الاسم بالفعل.");

        public static readonly Error UnsupportedStatusTransition =
            Error.Validation("Projects.UnsupportedStatusTransition", "تحويل حالة المشروع المطلوب غير مدعوم.");
    }

    public static class Expenses
    {
        public static readonly Error ExpenseNotFound =
            Error.NotFound("Expenses.ExpenseNotFound", "لم يتم العثور على المصروف.");

        public static readonly Error ExpenseReceiptNotFound =
            Error.NotFound("Expenses.ExpenseReceiptNotFound", "لم يتم العثور على إيصال المصروف.");
    }

    public static class Invoices
    {
        public static readonly Error InvoiceNotFound =
            Error.NotFound("Invoices.InvoiceNotFound", "لم يتم العثور على الفاتورة.");

        public static readonly Error InvoiceNumberAlreadyExists =
            Error.Conflict("Invoices.InvoiceNumberAlreadyExists", "توجد فاتورة بنفس الرقم بالفعل.");
    }

    public static class Pricing
    {
        public static readonly Error PricingPlanCannotBeUsed =
            Error.Validation("Pricing.PricingPlanCannotBeUsed", "لا يمكن استخدام خطة التسعير المحددة لهذا العرض.");
    }

    public static class Analyses
    {
        public static readonly Error AnalysisNotFound =
            Error.NotFound("Analyses.AnalysisNotFound", "لم يتم العثور على التحليل.");
    }

    public static class Notifications
    {
        public static readonly Error NotificationNotFound =
            Error.NotFound("Notifications.NotificationNotFound", "لم يتم العثور على الإشعار.");
    }

    public static class Auth
    {
        public static readonly Error EmailAlreadyRegistered =
            Error.Conflict("Auth.EmailAlreadyRegistered", "البريد الإلكتروني مسجل بالفعل.");

        public static readonly Error EmailVerificationNotConfigured =
            Error.Failure("Auth.EmailVerificationNotConfigured", "Email verification is not configured.");

        public static Error EmailVerificationNotConfiguredDetails(string details) =>
            Error.Failure("Auth.EmailVerificationNotConfigured", $"Email verification is not configured: {details}");

        public static readonly Error EmailVerificationSendFailed =
            Error.Failure("Auth.EmailVerificationSendFailed", "Unable to send the email verification code.");

        public static Error EmailVerificationSendFailedDetails(string details) =>
            Error.Failure("Auth.EmailVerificationSendFailed", $"Unable to send the email verification code: {details}");

        public static readonly Error InvalidEmailVerificationCode =
            Error.Validation("Auth.InvalidEmailVerificationCode", "Invalid or expired email verification code.");

        public static readonly Error EmailVerificationThrottled =
            Error.Validation("Auth.EmailVerificationThrottled", "يرجى الانتظار 5 دقائق قبل طلب رمز تحقق آخر.");

        public static readonly Error EmailVerificationTooManyRequests =
            Error.Validation("Auth.EmailVerificationTooManyRequests", "تم الوصول إلى الحد اليومي لرموز التحقق (5 مرات). يرجى المحاولة غداً.");

        public static Error RegistrationFailed(string details) =>
            Error.Failure("Auth.RegistrationFailed", $"فشل التسجيل: {details}");

        public static readonly Error InvalidCredentials =
            Error.Unauthorized("Auth.InvalidCredentials", "البريد الإلكتروني أو كلمة المرور غير صحيحة.");

        public static readonly Error AccountLocked =
            Error.Unauthorized("Auth.AccountLocked", "الحساب مقفل مؤقتاً. يرجى المحاولة لاحقاً.");

        public static readonly Error UserNotFound =
            Error.NotFound("Auth.UserNotFound", "لم يتم العثور على المستخدم.");

        public static readonly Error UpdateFailed =
            Error.Failure("Auth.UpdateFailed", "فشل تحديث الملف الشخصي.");

        public static readonly Error InvalidRefreshToken =
            Error.Unauthorized("Auth.InvalidRefreshToken", "رمز التحديث غير صالح.");

        public static readonly Error RefreshTokenReuse =
            Error.Unauthorized("Auth.RefreshTokenReuse", "تم اكتشاف إعادة استخدام رمز التحديث. يرجى تسجيل الدخول مجدداً.");

        public static readonly Error InvalidCurrentPassword =
            Error.Unauthorized("Auth.InvalidCurrentPassword", "كلمة المرور الحالية غير صحيحة.");

        public static Error PasswordChangeFailed(string details) =>
            Error.Validation("Auth.PasswordChangeFailed", $"فشل تغيير كلمة المرور: {details}");

        public static Error PasswordResetFailed(string details) =>
            Error.Validation("Auth.PasswordResetFailed", $"فشل إعادة تعيين كلمة المرور: {details}");
    }
}
