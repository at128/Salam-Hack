using Asp.Versioning;
using SalamHack.Application.Features.Auth.Commands.ChangePassword;
using SalamHack.Application.Features.Auth.Commands.ForgotPassword;
using SalamHack.Application.Features.Auth.Commands.Login;
using SalamHack.Application.Features.Auth.Commands.LogoutAllSessions;
using SalamHack.Application.Features.Auth.Commands.LogoutCurrentSession;
using SalamHack.Application.Features.Auth.Commands.RefreshToken;
using SalamHack.Application.Features.Auth.Commands.Register;
using SalamHack.Application.Features.Auth.Commands.ResetPassword;
using SalamHack.Application.Features.Auth.Commands.UpdateProfile;
using SalamHack.Application.Features.Auth.Commands.VerifyRegistration;
using SalamHack.Application.Features.Auth.Queries.GetProfile;
using SalamHack.Api.Responses;
using SalamHack.Contracts.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

public sealed class AuthController(ISender sender, ILogger<AuthController> logger) : ApiController
{
    /// <summary>Register a new user account.</summary>
    [EnableRateLimiting("auth")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<EmailVerificationChallengeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterCommand(
            request.Email, request.Password, request.FirstName,
            request.LastName, request.PhoneNumber), ct);

        return result.Match(
            response => OkResponse(
                new EmailVerificationChallengeResponse(response.Email, response.ExpiresInMinutes),
                "Verification code sent."),
            Problem);
    }

    /// <summary>Verify email OTP and create a new user account.</summary>
    [EnableRateLimiting("auth")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpPost("register/verify")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> VerifyRegistration(
        [FromBody] VerifyRegistrationRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new VerifyRegistrationCommand(
            request.Email, request.Password, request.FirstName,
            request.LastName, request.PhoneNumber, request.Otp), ct);

        return result.Match(
            response => CreatedResponse(nameof(GetProfile), null, response, "Registered successfully."),
            Problem);
    }

    /// <summary>Login and receive a JWT token.</summary>
    [EnableRateLimiting("auth")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var result = await sender.Send(
                new LoginCommand(request.Email, request.Password), ct);

            return result.Match(response => OkResponse(response), Problem);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected login endpoint failure for {Email}.", request.Email);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object?>.Fail(
                    $"Login failed with {ex.GetType().Name}: {ex.Message}",
                    [
                        new ApiErrorDto(
                            ex.GetType().FullName ?? ex.GetType().Name,
                            ex.ToString(),
                            "Unexpected")
                    ],
                    HttpContext.TraceIdentifier));
        }
    }

    /// <summary>Send a password reset OTP to the user's email.</summary>
    [EnableRateLimiting("auth")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<EmailVerificationChallengeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ForgotPasswordCommand(request.Email), ct);

        return result.Match(
            response => OkResponse(
                new EmailVerificationChallengeResponse(response.Email, response.ExpiresInMinutes),
                "If an account exists for this email, a reset code has been sent."),
            Problem);
    }

    /// <summary>Verify password reset OTP and set a new password.</summary>
    [EnableRateLimiting("auth")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ResetPasswordCommand(
            request.Email,
            request.Otp,
            request.NewPassword), ct);

        return result.Match(
            _ => OkResponse<object?>(null, "Password reset successfully."),
            Problem);
    }

    /// <summary>Get the current user's profile.</summary>
    [Authorize]
    [EnableRateLimiting("user-read")]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetProfileQuery(userId), ct);

        return result.Match(response => OkResponse(response), Problem);
    }

    /// <summary>Update the current user's profile.</summary>
    [Authorize]
    [EnableRateLimiting("user-write")]
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new UpdateProfileCommand(
            userId, request.FirstName, request.LastName, request.PhoneNumber,
            request.BankName, request.BankAccountName, request.BankIban), ct);

        return result.Match(response => OkResponse(response), Problem);
    }

    /// <summary>Change the current user's password.</summary>
    [Authorize]
    [EnableRateLimiting("user-write")]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new ChangePasswordCommand(
            userId,
            request.CurrentPassword,
            request.NewPassword), ct);

        return result.Match(
            _ => OkResponse<object?>(null, "Password changed successfully."),
            Problem);
    }

    [EnableRateLimiting("auth-refresh")]
    [HttpPost("refresh")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var result = await sender.Send(new RefreshTokenCommand(), ct);
        return result.Match(response => OkResponse(response), Problem);
    }

    [Authorize]
    [EnableRateLimiting("user-write")]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        await sender.Send(new LogoutCurrentSessionCommand(userId), ct);
        return OkResponse<object?>(null, "Logged out successfully.");
    }

    [Authorize]
    [EnableRateLimiting("user-write")]
    [HttpPost("logout-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAllSessions(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        await sender.Send(new LogoutAllSessionsCommand(userId), ct);
        return OkResponse<object?>(null, "All sessions logged out successfully.");
    }
}
