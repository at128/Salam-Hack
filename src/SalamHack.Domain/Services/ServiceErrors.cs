using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Services;

public static class ServiceErrors
{
    public static readonly Error InvalidUserId = Error.Validation(
        "Service.InvalidUserId",
        "معرف المستخدم مطلوب.");

    public static readonly Error ServiceNameRequired = Error.Validation(
        "Service.ServiceNameRequired",
        "اسم الخدمة مطلوب.");

    public static readonly Error DefaultHourlyRateMustBePositive = Error.Validation(
        "Service.DefaultHourlyRateMustBePositive",
        "يجب أن يكون سعر الساعة الافتراضي أكبر من صفر.");

    public static readonly Error DefaultRevisionsCannotBeNegative = Error.Validation(
        "Service.DefaultRevisionsCannotBeNegative",
        "لا يمكن أن يكون عدد المراجعات الافتراضي سالباً.");
}
