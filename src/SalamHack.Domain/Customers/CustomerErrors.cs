using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Customers;

public static class CustomerErrors
{
    public static readonly Error InvalidUserId = Error.Validation(
        "Customer.InvalidUserId",
        "User id is required.");

    public static readonly Error CustomerNameRequired = Error.Validation(
        "Customer.CustomerNameRequired",
        "Customer name is required.");

    public static readonly Error EmailRequired = Error.Validation(
        "Customer.EmailRequired",
        "Email is required.");

    public static readonly Error PhoneRequired = Error.Validation(
        "Customer.PhoneRequired",
        "Phone is required.");
}
