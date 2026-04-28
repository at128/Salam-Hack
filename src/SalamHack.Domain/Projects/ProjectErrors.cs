using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Projects;

public static class ProjectErrors
{
    public static readonly Error CannotStartCancelledProject = Error.Failure(
        "Project.CannotStartCancelledProject",
        "Cannot start a cancelled project.");

    public static readonly Error CannotCompleteCancelledProject = Error.Failure(
        "Project.CannotCompleteCancelledProject",
        "Cannot complete a cancelled project.");

    public static readonly Error CannotCancelCompletedProject = Error.Failure(
        "Project.CannotCancelCompletedProject",
        "Cannot cancel a completed project.");

    public static readonly Error AlreadyInProgress = Error.Failure(
        "Project.AlreadyInProgress",
        "Project is already in progress.");

    public static readonly Error AlreadyCompleted = Error.Failure(
        "Project.AlreadyCompleted",
        "Project is already completed.");

    public static readonly Error AlreadyCancelled = Error.Failure(
        "Project.AlreadyCancelled",
        "Project is already cancelled.");

    public static readonly Error PriceCannotBeZero = Error.Validation(
        "Project.PriceCannotBeZero",
        "Project price cannot be zero when calculating health.");

    public static readonly Error ProjectNameRequired = Error.Validation(
        "Project.ProjectNameRequired",
        "Project name is required.");

    public static readonly Error InvalidUserId = Error.Validation(
        "Project.InvalidUserId",
        "User id is required.");

    public static readonly Error InvalidCustomerId = Error.Validation(
        "Project.InvalidCustomerId",
        "Customer id is required.");

    public static readonly Error InvalidServiceId = Error.Validation(
        "Project.InvalidServiceId",
        "Service id is required.");

    public static readonly Error EstimatedHoursMustBePositive = Error.Validation(
        "Project.EstimatedHoursMustBePositive",
        "Estimated hours must be greater than zero.");

    public static readonly Error ToolCostCannotBeNegative = Error.Validation(
        "Project.ToolCostCannotBeNegative",
        "Tool cost cannot be negative.");

    public static readonly Error RevisionCannotBeNegative = Error.Validation(
        "Project.RevisionCannotBeNegative",
        "Revision cannot be negative.");

    public static readonly Error SuggestedPriceMustBePositive = Error.Validation(
        "Project.SuggestedPriceMustBePositive",
        "Suggested price must be greater than zero.");

    public static readonly Error EndDateBeforeStartDate = Error.Validation(
        "Project.EndDateBeforeStartDate",
        "End date cannot be earlier than start date.");

    public static readonly Error ActualHoursCannotBeNegative = Error.Validation(
        "Project.ActualHoursCannotBeNegative",
        "Actual hours cannot be negative.");

    public static readonly Error AdditionalExpensesCannotBeNegative = Error.Validation(
        "Project.AdditionalExpensesCannotBeNegative",
        "Additional expenses cannot be negative.");

    public static readonly Error NotFound = Error.NotFound(
        "Project.NotFound",
        "Project was not found.");
}
