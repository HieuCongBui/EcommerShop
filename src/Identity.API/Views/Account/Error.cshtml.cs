using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace Identity.API.Views.Account
{
    [AllowAnonymous]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorUri { get; set; }
        public string ErrorTitle { get; set; }
        public string ReturnUrl { get; set; }
        public string HomeUrl { get; set; }
        public string RequestId { get; set; }
        public string State { get; set; }
        public bool ShowTechnicalDetails { get; set; }

        public IActionResult OnGet(
            string error = null,
            string error_description = null,
            string error_uri = null,
            string returnUrl = null,
            string state = null)
        {
            try
            {
                Error = error;
                ErrorDescription = error_description;
                ErrorUri = error_uri;
                ReturnUrl = returnUrl;
                State = state;
                RequestId = HttpContext.TraceIdentifier;
                ShowTechnicalDetails = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
                HomeUrl = Url.Content("~/");

                // Set user-friendly error title and description based on error code
                SetUserFriendlyErrorMessage();

                // Log the error for monitoring
                _logger.LogWarning("OAuth2 Error displayed: {Error} - {ErrorDescription} - RequestId: {RequestId}", 
                    error, error_description, RequestId);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading error page");
                
                // Fallback error handling
                ErrorTitle = "An Error Occurred";
                ErrorDescription = "We're sorry, but an unexpected error occurred while processing your request.";
                Error = "server_error";
                
                return Page();
            }
        }

        private void SetUserFriendlyErrorMessage()
        {
            (ErrorTitle, var userFriendlyDescription) = Error?.ToLowerInvariant() switch
            {
                "access_denied" => (
                    "Access Denied",
                    "You denied access to the application. If this was unintentional, you can try again."
                ),
                "invalid_request" => (
                    "Invalid Request",
                    "The authorization request is malformed or contains invalid parameters."
                ),
                "invalid_client" => (
                    "Invalid Client",
                    "The client application is not recognized or is not authorized to make this request."
                ),
                "invalid_grant" => (
                    "Invalid Grant",
                    "The authorization code or refresh token is invalid, expired, or has already been used."
                ),
                "unauthorized_client" => (
                    "Unauthorized Client",
                    "This client is not authorized to use the requested authentication method."
                ),
                "unsupported_grant_type" => (
                    "Unsupported Grant Type",
                    "The authorization server does not support the requested grant type."
                ),
                "invalid_scope" => (
                    "Invalid Scope",
                    "The requested scope is invalid, unknown, or exceeds the scope granted by the user."
                ),
                "server_error" => (
                    "Server Error",
                    "The authorization server encountered an unexpected condition that prevented it from fulfilling the request."
                ),
                "temporarily_unavailable" => (
                    "Service Temporarily Unavailable",
                    "The authorization server is currently unable to handle the request due to a temporary overloading or maintenance."
                ),
                "interaction_required" => (
                    "Interaction Required",
                    "User interaction is required to complete the authentication process."
                ),
                "login_required" => (
                    "Login Required",
                    "You need to log in to continue with the authorization process."
                ),
                "consent_required" => (
                    "Consent Required",
                    "User consent is required to complete the authorization process."
                ),
                "invalid_request_uri" => (
                    "Invalid Request URI",
                    "The request_uri parameter contains an invalid URI or the URI is not allowed."
                ),
                "invalid_request_object" => (
                    "Invalid Request Object",
                    "The request parameter contains an invalid request object."
                ),
                "request_not_supported" => (
                    "Request Not Supported",
                    "The authorization server does not support the use of the request parameter."
                ),
                "request_uri_not_supported" => (
                    "Request URI Not Supported",
                    "The authorization server does not support the use of the request_uri parameter."
                ),
                "registration_not_supported" => (
                    "Registration Not Supported",
                    "Dynamic client registration is not supported by this authorization server."
                ),
                _ => (
                    "Authentication Error",
                    "An error occurred during the authentication process. Please try again."
                )
            };

            // Use provided error description if available, otherwise use user-friendly description
            if (string.IsNullOrEmpty(ErrorDescription))
            {
                ErrorDescription = userFriendlyDescription;
            }
            else
            {
                // Keep the original technical description but make it more readable
                ErrorDescription = ErrorDescription.Replace("_", " ").Replace("+", " ");
                
                // Capitalize first letter
                if (!string.IsNullOrEmpty(ErrorDescription))
                {
                    ErrorDescription = char.ToUpper(ErrorDescription[0]) + ErrorDescription.Substring(1);
                }
            }
        }
    }
}