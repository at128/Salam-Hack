using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Common.Results;
using Microsoft.AspNetCore.Identity;

namespace SalamHack.Infrastructure.Identity;

public class IdentityService(
    UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<bool> IsEmailUniqueAsync(
        string email, CancellationToken ct = default)
    {
        var existing = await userManager.FindByEmailAsync(email);
        return existing is null;
    }

    public async Task<Result<UserAuthResult>> RegisterUserAsync(
    string email,
    string password,
    string firstName,
    string lastName,
    string? phoneNumber,
    CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email.Trim(),
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var identityResult = await userManager.CreateAsync(user, password);

        if (!identityResult.Succeeded)
        {
            if (identityResult.Errors.Any(e =>
                string.Equals(e.Code, "DuplicateEmail", StringComparison.OrdinalIgnoreCase)))
            {
                return ApplicationErrors.Auth.EmailAlreadyRegistered;
            }

            if (identityResult.Errors.Any(e =>
                string.Equals(e.Code, "DuplicateUserName", StringComparison.OrdinalIgnoreCase)))
            {
                return ApplicationErrors.Auth.EmailAlreadyRegistered;
            }

            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            return ApplicationErrors.Auth.RegistrationFailed(errors);
        }

        var roleResult = await userManager.AddToRoleAsync(user, ApplicationConstants.Roles.User);

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return ApplicationErrors.Auth.RegistrationFailed(
                $"User created but role assignment failed: {roleErrors}");
        }

        return new UserAuthResult(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            [ApplicationConstants.Roles.User],
            user.CreatedAtUtc);
    }

    public async Task<Result<UserAuthResult>> ValidateCredentialsAsync(
    string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return ApplicationErrors.Auth.InvalidCredentials;

        if (await userManager.IsLockedOutAsync(user))
            return ApplicationErrors.Auth.AccountLocked;

        var isValid = await userManager.CheckPasswordAsync(user, password);
        if (!isValid)
        {
            await userManager.AccessFailedAsync(user);
            return ApplicationErrors.Auth.InvalidCredentials;
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);

        return new UserAuthResult(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles,
            user.CreatedAtUtc);
    }

    public async Task<Result<UserProfileResult>> GetUserByIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ApplicationErrors.Auth.UserNotFound;

        var roles = await userManager.GetRolesAsync(user);

        return new UserProfileResult(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.PhoneNumber, user.BankName, user.BankAccountName, user.BankIban,
            roles.FirstOrDefault() ?? ApplicationConstants.Roles.User,
            user.CreatedAtUtc, user.UpdatedAtUtc);
    }

    public async Task<Result<UserProfileResult>> UpdateUserAsync(
        Guid userId, string firstName, string lastName,
        string? phoneNumber,
        string? bankName,
        string? bankAccountName,
        string? bankIban,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ApplicationErrors.Auth.UserNotFound;

        user.FirstName = firstName;
        user.LastName = lastName;
        user.PhoneNumber = NormalizeOptional(phoneNumber);
        user.BankName = NormalizeOptional(bankName);
        user.BankAccountName = NormalizeOptional(bankAccountName);
        user.BankIban = NormalizeOptional(bankIban);
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return ApplicationErrors.Auth.UpdateFailed;

        var roles = await userManager.GetRolesAsync(user);

        return new UserProfileResult(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.PhoneNumber, user.BankName, user.BankAccountName, user.BankIban,
            roles.FirstOrDefault() ?? ApplicationConstants.Roles.User,
            user.CreatedAtUtc, user.UpdatedAtUtc);
    }

    public async Task<Result<Success>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ApplicationErrors.Auth.UserNotFound;

        user.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var changeResult = await userManager.ChangePasswordAsync(
            user,
            currentPassword,
            newPassword);

        if (!changeResult.Succeeded)
        {
            if (changeResult.Errors.Any(e =>
                string.Equals(e.Code, "PasswordMismatch", StringComparison.OrdinalIgnoreCase)))
            {
                return ApplicationErrors.Auth.InvalidCurrentPassword;
            }

            var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
            return ApplicationErrors.Auth.PasswordChangeFailed(errors);
        }

        return Result.Success;
    }

    public async Task<Result<Success>> ResetPasswordAsync(
        string email,
        string newPassword,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return ApplicationErrors.Auth.UserNotFound;

        user.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            return ApplicationErrors.Auth.PasswordResetFailed(errors);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return Result.Success;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
