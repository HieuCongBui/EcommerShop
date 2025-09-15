using Identity.API.Models;
using Identity.API.Models.DTOs;
using Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailService emailService,
    IPasswordSecurityService passwordSecurityService,
    ILogger<AccountController> logger,
    IConfiguration configuration) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IEmailService _emailService = emailService;
    private readonly IPasswordSecurityService _passwordSecurityService = passwordSecurityService;
    private readonly ILogger<AccountController> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Invalid input", modelErrors));
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult("User with this email already exists"));
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var resultErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Failed to create user", resultErrors));
            }

            // Save password history
            await _passwordSecurityService.SavePasswordHistoryAsync(user, user.PasswordHash!);

            // Add user to default role
            await _userManager.AddToRoleAsync(user, "User");

            // Generate email confirmation token
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(emailConfirmationToken);
            var baseUrl = _configuration["ApplicationSettings:BaseUrl"];
            
            // Generate tracking token for initial registration
            var trackingToken = Guid.NewGuid().ToString();
            var confirmationLink = $"{baseUrl}/api/account/confirm-email?userId={user.Id}&token={encodedToken}&tracking={trackingToken}";

            // Initialize tracking data
            user.EmailConfirmationSentAt = DateTime.UtcNow;
            user.EmailConfirmationAttempts = 1;
            user.LastEmailConfirmationToken = trackingToken;
            await _userManager.UpdateAsync(user);

            // Send confirmation email
            await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                Roles = new List<string> { "User" }
            };

            _logger.LogInformation("User {Email} registered successfully", user.Email);
            return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "User registered successfully. Please check your email to confirm your account."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Login([FromBody] LoginDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Invalid input", modelErrors));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Invalid email or password"));
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Email not confirmed. Please check your email and confirm your account."));
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Roles = roles.ToList()
                };

                _logger.LogInformation("User {Email} logged in successfully", user.Email);
                return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "Login successful"));
            }

            if (result.IsLockedOut)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Account locked due to multiple failed login attempts. Please try again later."));
            }

            if (result.IsNotAllowed)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Login not allowed. Please confirm your email address."));
            }

            return BadRequest(ApiResponse<UserDto>.ErrorResult("Invalid email or password"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user login");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Confirm email address with click tracking
    /// </summary>
    [HttpGet("confirm-email")]
    public async Task<ActionResult<ApiResponse>> ConfirmEmail(string userId, string token, string? tracking = null)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(ApiResponse.ErrorResult("Invalid email confirmation parameters"));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(ApiResponse.ErrorResult("User not found"));
            }

            // Track the click
            if (!string.IsNullOrEmpty(tracking))
            {
                user.EmailConfirmationClickedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Log the click for analytics
                _logger.LogInformation("Email confirmation link clicked by user {Email} with tracking {Tracking}", 
                    user.Email, tracking);
            }

            if (user.EmailConfirmed)
            {
                return Ok(ApiResponse.SuccessResult("Email already confirmed"));
            }

            var decodedToken = HttpUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            
            if (result.Succeeded)
            {
                // Update completion time
                user.EmailConfirmationCompletedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Email confirmed for user {Email}", user.Email);
                return Ok(ApiResponse.SuccessResult("Email confirmed successfully. You can now log in."));
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponse.ErrorResult("Failed to confirm email", errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during email confirmation");
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Invalid input", modelErrors));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.EmailConfirmed)
            {
                // Don't reveal that the user doesn't exist or email is not confirmed
                return Ok(ApiResponse.SuccessResult("If the email exists and is confirmed, a password reset link has been sent."));
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(resetToken);
            var baseUrl = _configuration["ApplicationSettings:BaseUrl"];
            var resetLink = $"{baseUrl}/reset-password?email={HttpUtility.UrlEncode(user.Email)}&token={encodedToken}";

            await _emailService.SendPasswordResetAsync(user.Email, resetLink);

            _logger.LogInformation("Password reset email sent to {Email}", user.Email);
            return Ok(ApiResponse.SuccessResult("If the email exists and is confirmed, a password reset link has been sent."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during forgot password request");
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                            var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Invalid input", modelErrors));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(ApiResponse.ErrorResult("Invalid reset password request"));
            }

            var decodedToken = HttpUtility.UrlDecode(model.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);
            
            if (result.Succeeded)
            {
                // Save password history
                await _passwordSecurityService.SavePasswordHistoryAsync(user, user.PasswordHash!);

                _logger.LogInformation("Password reset successfully for user {Email}", user.Email);
                return Ok(ApiResponse.SuccessResult("Password reset successfully. You can now log in with your new password."));
            }

            var resultErrors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponse.ErrorResult("Failed to reset password", resultErrors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password reset");
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Invalid input", modelErrors));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(ApiResponse.ErrorResult("User not found"));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                // Save password history
                await _passwordSecurityService.SavePasswordHistoryAsync(user, user.PasswordHash!);

                _logger.LogInformation("Password changed successfully for user {Email}", user.Email);
                return Ok(ApiResponse.SuccessResult("Password changed successfully"));
            }

            var resultErrors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponse.ErrorResult("Failed to change password", resultErrors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password change");
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// User logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully");
            return Ok(ApiResponse.SuccessResult("Logged out successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResult("User not found"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList()
            };

            return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "Profile retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user profile");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get email confirmation status with tracking details
    /// </summary>
    [HttpGet("email-confirmation-status")]
    public async Task<ActionResult<ApiResponse<EmailConfirmationStatusDto>>> GetEmailConfirmationStatus(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(ApiResponse<EmailConfirmationStatusDto>.ErrorResult("Email is required"));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that user doesn't exist
                return Ok(ApiResponse<EmailConfirmationStatusDto>.SuccessResult(
                    new EmailConfirmationStatusDto { IsConfirmed = false }, 
                    "Email confirmation status retrieved"));
            }

            var status = new EmailConfirmationStatusDto
            {
                IsConfirmed = user.EmailConfirmed,
                EmailSentAt = user.EmailConfirmationSentAt,
                EmailClickedAt = user.EmailConfirmationClickedAt,
                EmailConfirmedAt = user.EmailConfirmationCompletedAt,
                AttemptCount = user.EmailConfirmationAttempts,
                HasClickedLink = user.EmailConfirmationClickedAt.HasValue
            };

            return Ok(ApiResponse<EmailConfirmationStatusDto>.SuccessResult(status, "Email confirmation status retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking email confirmation status");
            return StatusCode(500, ApiResponse<EmailConfirmationStatusDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Resend email confirmation with tracking
    /// </summary>
    [HttpPost("resend-confirmation")]
    public async Task<ActionResult<ApiResponse>> ResendEmailConfirmation([FromBody] ForgotPasswordDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Invalid input", modelErrors));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return Ok(ApiResponse.SuccessResult("If the email exists, a confirmation email has been sent."));
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(ApiResponse.ErrorResult("Email is already confirmed"));
            }

            // Check rate limiting - prevent too many resend attempts
            if (user.EmailConfirmationSentAt.HasValue && 
                user.EmailConfirmationSentAt.Value.AddMinutes(2) > DateTime.UtcNow)
            {
                return BadRequest(ApiResponse.ErrorResult("Please wait before requesting another confirmation email"));
            }

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(emailConfirmationToken);
            var baseUrl = _configuration["ApplicationSettings:BaseUrl"];
            
            // Generate tracking token
            var trackingToken = Guid.NewGuid().ToString();
            
            var confirmationLink = $"{baseUrl}/api/account/confirm-email?userId={user.Id}&token={encodedToken}&tracking={trackingToken}";

            // Update user tracking information
            user.EmailConfirmationSentAt = DateTime.UtcNow;
            user.EmailConfirmationAttempts++;
            user.LastEmailConfirmationToken = trackingToken;
            await _userManager.UpdateAsync(user);

            await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);

            _logger.LogInformation("Email confirmation resent to {Email}, attempt {Attempts}", user.Email, user.EmailConfirmationAttempts);
            return Ok(ApiResponse.SuccessResult("If the email exists, a confirmation email has been sent."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resending email confirmation");
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }
}