using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace Identity.API.Views.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            try
            {
                // Clear existing external cookies to ensure clean authentication
                try
                {
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                }
                catch
                {
                    // Ignore errors if no external authentication scheme is configured
                }

                ReturnUrl = returnUrl ?? Url.Content("~/");

                // Check if user is already authenticated
                if (User.Identity?.IsAuthenticated == true)
                {
                    return LocalRedirect(GetReturnUrl(returnUrl));
                }

                // Check for logout message
                if (Request.Query["message"] == "logout")
                {
                    SuccessMessage = "You have been successfully logged out.";
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading login page");
                ErrorMessage = "An error occurred while loading the page. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                return await ProcessLogin();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process for user: {Email}", Input.Email);
                ErrorMessage = "An unexpected error occurred. Please try again.";
                return Page();
            }
        }

        private async Task<IActionResult> ProcessLogin()
        {
            var email = Input.Email?.Trim();
            var password = Input.Password;
            var rememberMe = Input.RememberMe;

            // Basic validation
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Please enter both email/username and password.";
                return Page();
            }

            // Find user by email or username
            var user = await FindUserAsync(email);
            if (user == null)
            {
                ErrorMessage = "Invalid email/username or password.";
                _logger.LogWarning("Login attempt failed: User not found for {Email}", email);
                return Page();
            }

            // Check if email is confirmed (only if email confirmation is required)
            var emailConfirmationRequired = _userManager.Options.SignIn.RequireConfirmedEmail;
            if (emailConfirmationRequired && !await _userManager.IsEmailConfirmedAsync(user))
            {
                ErrorMessage = "Your email address is not confirmed. Please check your email and confirm your account.";
                _logger.LogWarning("Login attempt failed: Email not confirmed for {Email}", email);
                return Page();
            }

            // Attempt sign in
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, 
                password, 
                rememberMe, 
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully", email);
                
                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Redirect to return URL
                return LocalRedirect(GetReturnUrl(ReturnUrl));
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User account {Email} is locked out", email);
                ErrorMessage = "Your account has been locked due to multiple failed login attempts. Please try again later.";
            }
            else if (result.IsNotAllowed)
            {
                _logger.LogWarning("User {Email} is not allowed to sign in", email);
                ErrorMessage = "Your account is not allowed to sign in. Please contact support.";
            }
            else if (result.RequiresTwoFactor)
            {
                _logger.LogInformation("User {Email} requires two-factor authentication", email);
                ErrorMessage = "Two-factor authentication is required but not currently supported in this interface.";
            }
            else
            {
                _logger.LogWarning("Login attempt failed for {Email}", email);
                ErrorMessage = "Invalid email/username or password.";
            }

            return Page();
        }

        private async Task<ApplicationUser?> FindUserAsync(string emailOrUsername)
        {
            if (string.IsNullOrEmpty(emailOrUsername))
            {
                return null;
            }

            // First try to find by email
            var user = await _userManager.FindByEmailAsync(emailOrUsername);
            if (user != null)
            {
                return user;
            }

            // If not found by email, try by username
            user = await _userManager.FindByNameAsync(emailOrUsername);
            return user;
        }

        private string GetReturnUrl(string? returnUrl)
        {
            // Validate return URL to prevent open redirect attacks
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            // Default redirect
            return Url.Content("~/");
        }

        public class LoginInputModel
        {
            [Required(ErrorMessage = "Email or username is required.")]
            [Display(Name = "Email or Username")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }
    }
}