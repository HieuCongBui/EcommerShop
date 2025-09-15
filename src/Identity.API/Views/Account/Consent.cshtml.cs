using Identity.API.Models;
using Identity.API.Models.ViewModels;
using Identity.API.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore;
using OpenIddict.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.API.Views.Account
{
    [Authorize]
    public class ConsentModel : PageModel
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictScopeManager _scopeManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ConsentModel> _logger;

        public ConsentModel(
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictScopeManager scopeManager,
            IOpenIddictAuthorizationManager authorizationManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ConsentModel> logger)
        {
            _applicationManager = applicationManager;
            _scopeManager = scopeManager;
            _authorizationManager = authorizationManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public ConsentInputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public AuthorizeViewModel? ConsentData { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            try
            {
                ReturnUrl = returnUrl ?? Url.Content("~/");

                // Get the OpenIddict request from the HTTP context
                var request = HttpContext.GetOpenIddictServerRequest();
                if (request == null)
                {
                    ErrorMessage = "Invalid OAuth request.";
                    return Page();
                }

                // Load consent data
                ConsentData = await LoadConsentDataAsync(request);
                if (ConsentData == null)
                {
                    ErrorMessage = "Unable to load application information.";
                    return Page();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consent page");
                ErrorMessage = "An error occurred while loading the consent page. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                // Reload consent data if validation fails
                var request = HttpContext.GetOpenIddictServerRequest();
                if (request != null)
                {
                    ConsentData = await LoadConsentDataAsync(request);
                }
                return Page();
            }

            try
            {
                return await ProcessConsentAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing consent");
                ErrorMessage = "An unexpected error occurred. Please try again.";
                
                // Reload consent data
                var request = HttpContext.GetOpenIddictServerRequest();
                if (request != null)
                {
                    ConsentData = await LoadConsentDataAsync(request);
                }
                return Page();
            }
        }

        private async Task<IActionResult> ProcessConsentAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                ErrorMessage = "Invalid OAuth request.";
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // If user denied consent
            if (Input.Action == "deny")
            {
                _logger.LogInformation("User {UserId} denied consent for client {ClientId}", 
                    user.Id, request.ClientId);

                // Redirect back to the authorization endpoint with access_denied error
                var queryString = Request.QueryString.HasValue ? Request.QueryString.Value : "";
                return Redirect($"/connect/authorize{queryString}&error=access_denied&error_description=The+user+denied+the+request");
            }

            // If user allowed consent
            if (Input.Action == "allow")
            {
                _logger.LogInformation("User {UserId} granted consent for client {ClientId}", 
                    user.Id, request.ClientId);

                // Create permanent authorization if remember consent is checked
                if (Input.RememberConsent)
                {
                    await CreatePermanentAuthorizationAsync(user, request);
                }

                // Redirect back to the authorization endpoint to complete the flow
                var queryString = Request.QueryString.HasValue ? Request.QueryString.Value : "";
                return Redirect($"/connect/authorize{queryString}");
            }

            ErrorMessage = "Invalid action specified.";
            ConsentData = await LoadConsentDataAsync(request);
            return Page();
        }

        private async Task<AuthorizeViewModel?> LoadConsentDataAsync(OpenIddictRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ClientId))
                {
                    return null;
                }

                var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
                if (application == null)
                {
                    return null;
                }

                var scopes = request.GetScopes();
                var scopeDescriptions = new List<ScopeDescription>();

                foreach (var scopeName in scopes)
                {
                    var scope = await _scopeManager.FindByNameAsync(scopeName);
                    if (scope != null)
                    {
                        scopeDescriptions.Add(new ScopeDescription
                        {
                            Name = scopeName,
                            DisplayName = await _scopeManager.GetDisplayNameAsync(scope) ?? scopeName,
                            Description = await _scopeManager.GetDescriptionAsync(scope) ?? GetDefaultScopeDescription(scopeName)
                        });
                    }
                    else
                    {
                        // Add default scope description for built-in scopes
                        scopeDescriptions.Add(new ScopeDescription
                        {
                            Name = scopeName,
                            DisplayName = GetDefaultScopeDisplayName(scopeName),
                            Description = GetDefaultScopeDescription(scopeName)
                        });
                    }
                }

                var applicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application);
                return new AuthorizeViewModel
                {
                    ApplicationName = applicationName,
                    Scope = string.Join(" ", scopes),
                    Scopes = scopes.ToList(),
                    ScopeDescriptions = scopeDescriptions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consent data for client {ClientId}", request.ClientId);
                return null;
            }
        }

        private async Task CreatePermanentAuthorizationAsync(ApplicationUser user, OpenIddictRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ClientId))
                {
                    return;
                }

                var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
                if (application == null)
                {
                    return;
                }

                var userId = await _userManager.GetUserIdAsync(user);
                var applicationId = await _applicationManager.GetIdAsync(application);

                // Check if permanent authorization already exists
                var existingAuthorizations = new List<object>();
                await foreach (var auth in _authorizationManager.FindAsync(
                    subject: userId,
                    client: applicationId,
                    status: Statuses.Valid,
                    type: AuthorizationTypes.Permanent,
                    scopes: request.GetScopes()))
                {
                    existingAuthorizations.Add(auth);
                }

                if (!existingAuthorizations.Any())
                {
                    var principal = new ClaimsPrincipal(new ClaimsIdentity())
                        .SetClaim(Claims.Subject, userId);

                    await _authorizationManager.CreateAsync(
                        principal: principal,
                        subject: userId,
                        client: applicationId,
                        type: AuthorizationTypes.Permanent,
                        scopes: request.GetScopes());

                    _logger.LogInformation("Created permanent authorization for user {UserId} and client {ClientId}", 
                        user.Id, request.ClientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permanent authorization for user {UserId} and client {ClientId}", 
                    user.Id, request.ClientId);
            }
        }

        private static string GetDefaultScopeDisplayName(string scopeName)
        {
            return scopeName switch
            {
                "openid" => "OpenID Connect",
                "profile" => "Profile Information",
                "email" => "Email Address",
                "catalog" => "Catalog Access",
                "roles" => "User Roles",
                _ => scopeName
            };
        }

        private static string GetDefaultScopeDescription(string scopeName)
        {
            return scopeName switch
            {
                "openid" => "Access to your unique identifier",
                "profile" => "Access to your profile information (name, etc.)",
                "email" => "Access to your email address",
                "catalog" => "Access to catalog information and products",
                "roles" => "Access to your role and permission information",
                _ => $"Access to {scopeName} resources"
            };
        }

        public class ConsentInputModel
        {
            [Required]
            public string Action { get; set; } = string.Empty; // "allow" or "deny"

            public bool RememberConsent { get; set; }
        }
    }
}