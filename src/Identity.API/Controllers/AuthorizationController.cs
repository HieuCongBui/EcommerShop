using Identity.API.Models;
using Identity.API.Models.DTOs;
using Identity.API.Models.ViewModels;
using Identity.API.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.API.Controllers;

[ApiController]
[Route("connect")]
public class AuthorizationController : ControllerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<AuthorizationController> _logger;

    public AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IClaimsService claimsService,
        ILogger<AuthorizationController> logger)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
        _claimsService = claimsService;
        _logger = logger;
    }

    [HttpGet("authorize")]
    [HttpPost("authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        try
        {
            // Retrieve the user principal stored in the authentication cookie
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

            // If the user principal can't be extracted or the cookie is expired, redirect to login
            if (!result.Succeeded || string.IsNullOrEmpty(result.Principal?.Identity?.Name))
            {
                // If the client application requested promptless authentication,
                // return an error indicating that the user is not logged in
                if (HasPrompt(request, "none"))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                        }));
                }

                // Build a return URL based on the original request
                var returnUrl = Request.PathBase + Request.Path + QueryString.Create(
                    Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList());

                // Redirect to login page
                return Challenge(
                    authenticationSchemes: IdentityConstants.ApplicationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = returnUrl
                    });
            }

            // Retrieve the profile of the logged in user
            var user = await _userManager.GetUserAsync(result.Principal) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            // Retrieve the application details from the database
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            // Retrieve the permanent authorizations associated with the user and the calling client application
            var authorizationsList = new List<object>();
            await foreach (var auth in _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client: await _applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()))
            {
                authorizationsList.Add(auth);
            }

            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                // If the consent is external (e.g when authorizations are granted by a sysadmin),
                // immediately return an error if no authorization can be found in the database
                case ConsentTypes.External when !authorizationsList.Any():
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access this client application."
                        }));

                // If the consent is implicit or if an authorization was found,
                // return an authorization response without displaying the consent form
                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizationsList.Any():
                case ConsentTypes.Explicit when authorizationsList.Any() && !HasPrompt(request, "consent"):
                    return await IssueAuthorizationCodeAsync(user, request);

                // At this point, no authorization was found in the database and an error must be returned
                // if the client application specified prompt=none in the authorization request
                case ConsentTypes.Explicit when HasPrompt(request, "none"):
                case ConsentTypes.Systematic when HasPrompt(request, "none"):
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Interactive user consent is required."
                        }));

                // In every other case, render the consent form
                default:
                    return await ShowConsentFormAsync(request, user);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the authorization request");
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "An internal error has occurred"
                }));
        }
    }

    [HttpPost("consent")]
    [Authorize]
    public async Task<IActionResult> Consent([FromForm] ConsentDto model)
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        try
        {
            // Retrieve the profile of the logged in user
            var user = await _userManager.GetUserAsync(User) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            // If the user denied the consent, return an access denied error
            if (model.Action == "deny")
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The authorization was denied by the user"
                    }));
            }

            // Retrieve the application details from the database
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
                throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            // Create a permanent authorization if the user checked "remember my choice"
            var authorizationsList = new List<object>();
            await foreach (var auth in _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client: await _applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()))
            {
                authorizationsList.Add(auth);
            }
            var authorization = authorizationsList.FirstOrDefault();

            if (authorization == null && model.RememberConsent)
            {
                authorization = await _authorizationManager.CreateAsync(
                    principal: new ClaimsPrincipal(new ClaimsIdentity())
                        .SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user)),
                    subject: await _userManager.GetUserIdAsync(user),
                    client: await _applicationManager.GetIdAsync(application),
                    type: AuthorizationTypes.Permanent,
                    scopes: request.GetScopes());
            }

            return await IssueAuthorizationCodeAsync(user, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the consent");
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "An internal error has occurred"
                }));
        }
    }

    [HttpPost("token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        try
        {
            if (request.IsAuthorizationCodeGrantType())
            {
                return await ExchangeAuthorizationCodeAsync(request);
            }
            else if (request.IsClientCredentialsGrantType())
            {
                return await ExchangeClientCredentialsAsync(request);
            }
            else if (request.IsRefreshTokenGrantType())
            {
                return await ExchangeRefreshTokenAsync(request);
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the token request");
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "An internal error has occurred"
                }));
        }
    }

    [HttpGet("userinfo")]
    [HttpPost("userinfo")]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Userinfo()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is no longer valid."
                    }));
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Note: the "sub" claim is a mandatory claim and must be included in the JSON response
                [Claims.Subject] = await _userManager.GetUserIdAsync(user)
            };

            if (User.HasScope(Scopes.Email))
            {
                claims[Claims.Email] = await _userManager.GetEmailAsync(user) ?? string.Empty;
                claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
            }

            if (User.HasScope(Scopes.Profile))
            {
                claims[Claims.Name] = $"{user.FirstName} {user.LastName}".Trim();
                claims[Claims.GivenName] = user.FirstName ?? string.Empty;
                claims[Claims.FamilyName] = user.LastName ?? string.Empty;
                claims[Claims.PreferredUsername] = await _userManager.GetUserNameAsync(user) ?? string.Empty;
                claims["created_at"] = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
                
                if (user.LastLoginAt.HasValue)
                {
                    claims["last_login_at"] = user.LastLoginAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
            }

            if (User.HasScope("roles"))
            {
                var roles = await _userManager.GetRolesAsync(user);
                claims[Claims.Role] = roles.ToArray();
                
                // Add permissions based on roles
                var userClaims = await _claimsService.GetUserClaimsAsync(user);
                var permissions = userClaims.Where(c => c.Type == "permission").Select(c => c.Value).ToArray();
                if (permissions.Any())
                {
                    claims["permissions"] = permissions;
                }
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the userinfo request");
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "An internal error has occurred"
                }));
        }
    }

    [HttpGet("logout")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromQuery] string? logoutId = null)
    {
        try
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
            
            var logoutViewModel = new LogoutViewModel
            {
                LogoutId = logoutId,
                PostLogoutRedirectUri = request?.PostLogoutRedirectUri,
                WasAuthenticated = user != null,
                UserDisplayName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
                UserEmail = user?.Email,
                Message = "You have been successfully logged out."
            };

            // For API clients requesting JSON
            if (Request.Headers.Accept.ToString().Contains("application/json"))
            {
                await _signInManager.SignOutAsync();
                return Ok(ApiResponse<LogoutViewModel>.SuccessResult(logoutViewModel, "Logout completed"));
            }

            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application or to
            // the RedirectUri specified in the authentication properties if none was set.
            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = logoutViewModel.PostLogoutRedirectUri ?? "/"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the logout request");
            return Redirect("/");
        }
    }

    private async Task<IActionResult> ShowConsentFormAsync(OpenIddictRequest request, ApplicationUser user)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

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
                    Description = await _scopeManager.GetDescriptionAsync(scope) ?? string.Empty
                });
            }
        }

        var viewModel = new AuthorizeViewModel
        {
            ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application),
            Scope = string.Join(" ", scopes),
            Scopes = scopes.ToList(),
            ScopeDescriptions = scopeDescriptions
        };

        // Return the consent view - this would typically be a Razor view
        // For API-only implementation, we'll return a JSON response
        return Ok(ApiResponse<AuthorizeViewModel>.SuccessResult(viewModel, "Consent required"));
    }

    private async Task<IActionResult> IssueAuthorizationCodeAsync(ApplicationUser user, OpenIddictRequest request)
    {
        // Create identity with custom claims
        var identity = await _claimsService.CreateIdentityAsync(user, request.GetScopes());

        // Set OpenIddict-specific properties
        var resourcesList = new List<string>();
        await foreach (var resource in _scopeManager.ListResourcesAsync(identity.GetScopes()))
        {
            resourcesList.Add(resource);
        }
        identity.SetResources(resourcesList);

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        var authorizationsList = new List<object>();
        await foreach (var auth in _authorizationManager.FindAsync(
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()))
        {
            authorizationsList.Add(auth);
        }
        var authorization = authorizationsList.FirstOrDefault();

        authorization ??= await _authorizationManager.CreateAsync(
            principal: new ClaimsPrincipal(identity).SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user)),
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application),
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(GetDestinations);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangeAuthorizationCodeAsync(OpenIddictRequest request)
    {
        // Retrieve the claims principal stored in the authorization code
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Retrieve the user profile corresponding to the authorization code
        var user = await _userManager.GetUserAsync(result.Principal);
        if (user == null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                }));
        }

        // Ensure the user is still allowed to sign in
        if (!await _signInManager.CanSignInAsync(user))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                }));
        }

        // Create fresh identity with updated claims
        var scopes = result.Principal.GetScopes();
        var identity = await _claimsService.CreateIdentityAsync(user, scopes);
        identity.SetDestinations(GetDestinations);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangeClientCredentialsAsync(OpenIddictRequest request)
    {
        // Note: the client credentials are automatically validated by OpenIddict:
        // if client_id or client_secret are invalid, this action won't be invoked.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Create a new ClaimsIdentity containing the claims that
        // will be used to create an access_token.
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Use the client_id as the subject identifier.
        identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application));
        identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));

        identity.SetScopes(request.GetScopes());
        
        var resourcesList = new List<string>();
        await foreach (var resource in _scopeManager.ListResourcesAsync(identity.GetScopes()))
        {
            resourcesList.Add(resource);
        }
        identity.SetResources(resourcesList);
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangeRefreshTokenAsync(OpenIddictRequest request)
    {
        // Retrieve the claims principal stored in the refresh token.
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Retrieve the user profile corresponding to the refresh token.
        var user = await _userManager.GetUserAsync(result.Principal);
        if (user == null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                }));
        }

        // Ensure the user is still allowed to sign in.
        if (!await _signInManager.CanSignInAsync(user))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                }));
        }

        // Create fresh identity with updated claims
        var scopes = result.Principal.GetScopes();
        var identity = await _claimsService.CreateIdentityAsync(user, scopes);
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static bool HasPrompt(OpenIddictRequest request, string prompt)
    {
        return request.Prompt != null && request.Prompt.Contains(prompt);
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
            case "permission":
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope("roles"))
                    yield return Destinations.IdentityToken;

                yield break;

            case "user_id":
            case "created_at":
            case "last_login_at":
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}