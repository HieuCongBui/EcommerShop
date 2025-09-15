namespace Identity.API.Models.ViewModels;

/// <summary>
/// ViewModel for OAuth2/OpenID Connect logout flow
/// </summary>
public class LogoutViewModel
{
    /// <summary>
    /// The post-logout redirect URI where the user should be redirected after logout
    /// </summary>
    public string? PostLogoutRedirectUri { get; set; }

    /// <summary>
    /// The name of the client application requesting the logout    
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    /// The client ID of the application requesting the logout
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// The logout ID for tracking the logout request (used by OpenIddict)
    /// </summary>
    public string? LogoutId { get; set; }

    /// <summary>
    /// Whether to show a logout confirmation prompt to the user
    /// </summary>
    public bool ShowLogoutPrompt { get; set; } = true;

    /// <summary>
    /// Whether to automatically redirect after logout without user interaction
    /// </summary>
    public bool AutoRedirect { get; set; }

    /// <summary>
    /// The sign out iframe URL for single logout across multiple applications
    /// </summary>
    public string? SignOutIFrameUrl { get; set; }

    /// <summary>
    /// Whether this is a federated logout (logout initiated by an external identity provider)
    /// </summary>
    public bool IsFederatedLogout { get; set; }

    /// <summary>
    /// The external authentication scheme used for federated logout
    /// </summary>
    public string? ExternalAuthenticationScheme { get; set; }

    /// <summary>
    /// List of client URLs that need to be notified of the logout (for front-channel logout)
    /// </summary>
    public List<string> FrontChannelLogoutUrls { get; set; } = new();

    /// <summary>
    /// The state parameter for maintaining state across the logout flow
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// The user's display name for personalized logout message
    /// </summary>
    public string? UserDisplayName { get; set; }

    /// <summary>
    /// The user's email address
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// Whether the logout was triggered by an external identity provider
    /// </summary>
    public bool TriggeredByExternalIdp { get; set; }

    /// <summary>
    /// Whether the user was authenticated when logout was initiated
    /// </summary>
    public bool WasAuthenticated { get; set; }

    /// <summary>
    /// The logout message to display to the user
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Whether the logout was successful
    /// </summary>
    public bool LogoutSuccessful { get; set; } = true;
}