using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Services;

public static class ServiceErrors
{
    public static readonly Error InvalidUserId = Error.Validation(
        "Service.InvalidUserId",
        "User id is required.");

    public static readonly Error ServiceNameRequired = Error.Validation(
        "Service.ServiceNameRequired",
        "Service name is required.");

    public static readonly Error DefaultHourlyRateMustBePositive = Error.Validation(
        "Service.DefaultHourlyRateMustBePositive",
        "Default hourly rate must be greater than zero.");

    public static readonly Error DefaultRevisionsCannotBeNegative = Error.Validation(
        "Service.DefaultRevisionsCannotBeNegative",
        "Default revisions cannot be negative.");
}
