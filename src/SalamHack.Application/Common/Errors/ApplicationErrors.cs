using SalamHack.Domain.Common.Results;

namespace SalamHack.Application.Common.Errors;

public static class ApplicationErrors
{
    public static class Customers
    {
        public static readonly Error CustomerNotFound =
            Error.NotFound("Customers.CustomerNotFound", "Customer was not found.");

        public static readonly Error EmailAlreadyExists =
            Error.Conflict("Customers.EmailAlreadyExists", "A customer with the same email already exists.");
    }

    public static class Services
    {
        public static readonly Error ServiceNotFound =
            Error.NotFound("Services.ServiceNotFound", "Service was not found.");

        public static readonly Error ServiceNameAlreadyExists =
            Error.Conflict("Services.ServiceNameAlreadyExists", "A service with the same name already exists.");

        public static readonly Error InactiveServiceCannotBeUsed =
            Error.Conflict("Services.InactiveServiceCannotBeUsed", "Inactive service cannot be used for a new project.");
    }

    public static class Projects
    {
        public static readonly Error ProjectNotFound =
            Error.NotFound("Projects.ProjectNotFound", "Project was not found.");

        public static readonly Error ProjectNameAlreadyExists =
            Error.Conflict("Projects.ProjectNameAlreadyExists", "A project with the same name already exists.");

        public static readonly Error UnsupportedStatusTransition =
            Error.Validation("Projects.UnsupportedStatusTransition", "Requested project status transition is not supported.");
    }

    public static class Expenses
    {
        public static readonly Error ExpenseNotFound =
            Error.NotFound("Expenses.ExpenseNotFound", "Expense was not found.");
    }

    public static class Auth
    {
        public static readonly Error EmailAlreadyRegistered =
            Error.Conflict("Auth.EmailAlreadyRegistered", "Email is already registered.");

        public static Error RegistrationFailed(string details) =>
            Error.Failure("Auth.RegistrationFailed", $"Registration failed: {details}");

        public static readonly Error InvalidCredentials =
            Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

        public static readonly Error AccountLocked =
            Error.Unauthorized("Auth.AccountLocked", "Account is temporarily locked. Please try again later.");

        public static readonly Error UserNotFound =
            Error.NotFound("Auth.UserNotFound", "User not found.");

        public static readonly Error UpdateFailed =
            Error.Failure("Auth.UpdateFailed", "Profile update failed.");

        public static readonly Error InvalidRefreshToken =
            Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is invalid.");

        public static readonly Error RefreshTokenReuse =
            Error.Unauthorized("Auth.RefreshTokenReuse", "Refresh token reuse detected. Please login again.");

        public static readonly Error InvalidCurrentPassword =
            Error.Unauthorized("Auth.InvalidCurrentPassword", "Current password is incorrect.");

        public static Error PasswordChangeFailed(string details) =>
            Error.Validation("Auth.PasswordChangeFailed", $"Password change failed: {details}");
    }
}
