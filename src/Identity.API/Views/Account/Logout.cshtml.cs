using Identity.API.Models;
using Identity.API.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Server.AspNetCore;

namespace Identity.API.Views.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public string LogoutId { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        public string ErrorMessage { get; set; }
        public bool AutoRedirect { get; set; }
        public LogoutViewModel LogoutData { get; set; }

        public async Task<IActionResult> OnGetAsync(string logoutId = null, string post_logout_redirect_uri = null)
        {
            try
            {
                LogoutId = logoutId;
                PostLogoutRedirectUri = post_logout_redirect_uri;

                // Get current user info before logout
                var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
                
                LogoutData = new LogoutViewModel
                {
                    LogoutId = logoutId,
                    PostLogoutRedirectUri = post_logout_redirect_uri,
                    WasAuthenticated = user != null,
                    UserDisplayName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
                    UserEmail = user?.Email,
                    Message = user != null ? null : "You have been successfully signed out."
                };

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading logout page");
                ErrorMessage = "An error occurred while loading the logout page.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string action, string logoutId = null, string post_logout_redirect_uri = null)
        {
            LogoutId = logoutId;
            PostLogoutRedirectUri = post_logout_redirect_uri;

            if (action == "logout")
            {
                try
                {
                    return await ProcessLogoutAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing logout");
                    ErrorMessage = "An error occurred during logout. Please try again.";
                    return Page();
                }
            }

            // If not logout action, redirect to home or return URL
            var redirectUri = GetSafeRedirectUri(post_logout_redirect_uri);
            return LocalRedirect(redirectUri);
        }

        private async Task<IActionResult> ProcessLogoutAsync()
        {
            var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
            
            if (user != null)
            {
                _logger.LogInformation("User {UserId} ({UserName}) is logging out", user.Id, user.UserName);
            }

            // Sign out the user
            await _signInManager.SignOutAsync();

            // Set logout completion data
            LogoutData = new LogoutViewModel
            {
                LogoutId = LogoutId,
                PostLogoutRedirectUri = PostLogoutRedirectUri,
                WasAuthenticated = false,
                UserDisplayName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
                UserEmail = user?.Email,
                Message = "You have been successfully signed out."
            };

            // Check if we should auto-redirect
            if (!string.IsNullOrEmpty(PostLogoutRedirectUri))
            {
                AutoRedirect = true;
            }

            // For OpenIddict logout flow, we need to return SignOut result
            if (!string.IsNullOrEmpty(LogoutId))
            {
                return SignOut(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = PostLogoutRedirectUri ?? Url.Content("~/")
                    });
            }

            // For regular logout, show confirmation page or redirect
            if (!string.IsNullOrEmpty(PostLogoutRedirectUri))
            {
                // Immediate redirect for direct logout requests
                return LocalRedirect(GetSafeRedirectUri(PostLogoutRedirectUri));
            }

            return Page();
        }

        private string GetSafeRedirectUri(string uri)
        {
            // Validate redirect URI to prevent open redirect attacks
            if (!string.IsNullOrEmpty(uri) && Url.IsLocalUrl(uri))
            {
                return uri;
            }

            // Default safe redirect
            return Url.Content("~/");
        }
    }
}