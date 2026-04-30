using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Projects;

public static class ProjectErrors
{
    public static readonly Error CannotStartCancelledProject = Error.Failure(
        "Project.CannotStartCancelledProject",
        "لا يمكن بدء مشروع ملغى.");

    public static readonly Error CannotCompleteCancelledProject = Error.Failure(
        "Project.CannotCompleteCancelledProject",
        "لا يمكن إكمال مشروع ملغى.");

    public static readonly Error CannotCancelCompletedProject = Error.Failure(
        "Project.CannotCancelCompletedProject",
        "لا يمكن إلغاء مشروع مكتمل.");

    public static readonly Error AlreadyInProgress = Error.Failure(
        "Project.AlreadyInProgress",
        "المشروع قيد التنفيذ بالفعل.");

    public static readonly Error AlreadyCompleted = Error.Failure(
        "Project.AlreadyCompleted",
        "المشروع مكتمل بالفعل.");

    public static readonly Error AlreadyCancelled = Error.Failure(
        "Project.AlreadyCancelled",
        "المشروع ملغى بالفعل.");

    public static readonly Error PriceCannotBeZero = Error.Validation(
        "Project.PriceCannotBeZero",
        "لا يمكن أن يكون سعر المشروع صفراً عند حساب الصحة.");

    public static readonly Error ProjectNameRequired = Error.Validation(
        "Project.ProjectNameRequired",
        "اسم المشروع مطلوب.");

    public static readonly Error InvalidUserId = Error.Validation(
        "Project.InvalidUserId",
        "معرف المستخدم مطلوب.");

    public static readonly Error InvalidCustomerId = Error.Validation(
        "Project.InvalidCustomerId",
        "معرف العميل مطلوب.");

    public static readonly Error InvalidServiceId = Error.Validation(
        "Project.InvalidServiceId",
        "معرف الخدمة مطلوب.");

    public static readonly Error EstimatedHoursMustBePositive = Error.Validation(
        "Project.EstimatedHoursMustBePositive",
        "يجب أن تكون الساعات المقدرة أكبر من صفر.");

    public static readonly Error ToolCostCannotBeNegative = Error.Validation(
        "Project.ToolCostCannotBeNegative",
        "لا يمكن أن تكون تكلفة الأدوات سالبة.");

    public static readonly Error RevisionCannotBeNegative = Error.Validation(
        "Project.RevisionCannotBeNegative",
        "لا يمكن أن تكون المراجعة سالبة.");

    public static readonly Error SuggestedPriceMustBePositive = Error.Validation(
        "Project.SuggestedPriceMustBePositive",
        "يجب أن يكون السعر المقترح أكبر من صفر.");

    public static readonly Error EndDateBeforeStartDate = Error.Validation(
        "Project.EndDateBeforeStartDate",
        "لا يمكن أن يكون تاريخ الانتهاء قبل تاريخ البدء.");

    public static readonly Error ActualHoursCannotBeNegative = Error.Validation(
        "Project.ActualHoursCannotBeNegative",
        "لا يمكن أن تكون الساعات الفعلية سالبة.");

    public static readonly Error AdditionalExpensesCannotBeNegative = Error.Validation(
        "Project.AdditionalExpensesCannotBeNegative",
        "لا يمكن أن تكون المصروفات الإضافية سالبة.");

    public static readonly Error NotFound = Error.NotFound(
        "Project.NotFound",
        "لم يتم العثور على المشروع.");
}
