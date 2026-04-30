using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Customers;

public static class CustomerErrors
{
    public static readonly Error InvalidUserId = Error.Validation(
        "Customer.InvalidUserId",
        "معرف المستخدم مطلوب.");

    public static readonly Error CustomerNameRequired = Error.Validation(
        "Customer.CustomerNameRequired",
        "اسم العميل مطلوب.");

    public static readonly Error EmailRequired = Error.Validation(
        "Customer.EmailRequired",
        "البريد الإلكتروني مطلوب.");

    public static readonly Error PhoneRequired = Error.Validation(
        "Customer.PhoneRequired",
        "رقم الهاتف مطلوب.");
}
