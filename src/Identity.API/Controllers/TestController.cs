using Microsoft.AspNetCore.Mvc;
using Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;
using Identity.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<TestController> _logger;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public TestController(
            IEmailService emailService, 
            ILogger<TestController> logger, 
            IOpenIddictApplicationManager applicationManager,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _emailService = emailService;
            _logger = logger;
            _applicationManager = applicationManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Get complete OpenID Connect testing flow instructions
        /// </summary>
        [HttpGet("openid-flow")]
        public IActionResult GetOpenIdFlow()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            
            return Ok(new
            {
                Success = true,
                Message = "OpenID Connect Test Flow Instructions",
                BaseUrl = baseUrl,
                TestFlow = new
                {
                    Step1_CreateTestClient = new
                    {
                        Endpoint = $"{baseUrl}/api/test/create-test-client",
                        Method = "POST",
                        Description = "Create a test OAuth2 client application"
                    },
                    Step2_RegisterUser = new
                    {
                        Endpoint = $"{baseUrl}/api/account/register",
                        Method = "POST",
                        Description = "Register a new user account",
                        SamplePayload = new
                        {
                            email = "test@example.com",
                            password = "Test123!@#",
                            firstName = "Test",
                            lastName = "User"
                        }
                    },
                    Step3_ConfirmEmail = new
                    {
                        Endpoint = $"{baseUrl}/api/account/confirm-email",
                        Method = "GET",
                        Description = "Confirm email address using token from email",
                        Parameters = "?userId={{userId}}&token={{token}}"
                    },
                    Step4_Authorization = new
                    {
                        Endpoint = $"{baseUrl}/connect/authorize",
                        Method = "GET",
                        Description = "Start OAuth2 authorization flow",
                        SampleUrl = GetSampleAuthorizationUrl(baseUrl),
                        Parameters = new
                        {
                            client_id = "test-client",
                            response_type = "code",
                            scope = "openid profile email roles",
                            redirect_uri = $"{baseUrl}/api/test/test-callback",
                            state = "random-state-value"
                        }
                    },
                    Step5_TokenExchange = new
                    {
                        Endpoint = $"{baseUrl}/connect/token",
                        Method = "POST",
                        Description = "Exchange authorization code for tokens",
                        ContentType = "application/x-www-form-urlencoded",
                        SamplePayload = new
                        {
                            grant_type = "authorization_code",
                            code = "{{authorization_code}}",
                            redirect_uri = $"{baseUrl}/api/test/test-callback",
                            client_id = "test-client",
                            client_secret = "test-secret"
                        }
                    },
                    Step6_UserInfo = new
                    {
                        Endpoint = $"{baseUrl}/connect/userinfo",
                        Method = "GET",
                        Description = "Get user information using access token",
                        Headers = new
                        {
                            Authorization = "Bearer {{access_token}}"
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Create test client application for OpenID testing
        /// </summary>
        [HttpPost("create-test-client")]
        public async Task<IActionResult> CreateTestClient()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var clientId = "test-client";
                
                // Check if client already exists
                var existingApp = await _applicationManager.FindByClientIdAsync(clientId);
                if (existingApp != null)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Test client already exists",
                        ClientId = clientId,
                        ExistingClient = new
                        {
                            Id = await _applicationManager.GetIdAsync(existingApp),
                            DisplayName = await _applicationManager.GetDisplayNameAsync(existingApp),
                            Type = await _applicationManager.GetClientTypeAsync(existingApp)
                        }
                    });
                }

                // Create new test client
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = clientId,
                    ClientSecret = "test-secret",
                    DisplayName = "Test OpenID Client",
                    ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                    RedirectUris = 
                    {
                        new Uri($"{baseUrl}/api/test/test-callback"),
                        new Uri($"{baseUrl}/swagger/oauth2-redirect.html")
                    },
                    PostLogoutRedirectUris =
                    {
                        new Uri($"{baseUrl}/")
                    },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles,
                        "permission:catalog:read"
                    }
                };

                var application = await _applicationManager.CreateAsync(descriptor);

                return Ok(new
                {
                    Success = true,
                    Message = "Test client created successfully",
                    Client = new
                    {
                        Id = await _applicationManager.GetIdAsync(application),
                        ClientId = clientId,
                        ClientSecret = "test-secret",
                        DisplayName = await _applicationManager.GetDisplayNameAsync(application),
                        RedirectUris = descriptor.RedirectUris.Select(u => u.ToString()).ToArray()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create test client");
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Test authorization endpoint with GET request
        /// </summary>
        [HttpGet("test-authorize-get")]
        public IActionResult TestAuthorizeGet()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var authorizeUrl = GetSampleAuthorizationUrl(baseUrl);

                return Ok(new
                {
                    Success = true,
                    Message = "Authorization endpoint test URL generated",
                    AuthorizeUrl = authorizeUrl,
                    Instructions = new[]
                    {
                        "1. First create a test client using POST /api/test/create-test-client",
                        "2. Register a user using POST /api/account/register",
                        "3. Confirm the user's email",
                        "4. Visit the AuthorizeUrl below in your browser",
                        "5. Login with your registered user credentials",
                        "6. Grant consent to the application",
                        "7. You'll be redirected with an authorization code"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test authorize URL generation failed");
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Simulate callback endpoint for testing
        /// </summary>
        [HttpGet("test-callback")]
        public IActionResult TestCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return Ok(new
                {
                    Success = false,
                    Error = error,
                    Message = "Authorization failed",
                    State = state
                });
            }

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Authorization code is missing"
                });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            
            return Ok(new
            {
                Success = true,
                Message = "? Authorization code received successfully!",
                AuthorizationCode = code,
                State = state,
                NextStep = new
                {
                    Description = "Now exchange this code for tokens using the token endpoint",
                    TokenEndpoint = $"{baseUrl}/connect/token",
                    Method = "POST",
                    ContentType = "application/x-www-form-urlencoded",
                    CurlExample = $@"curl -X POST {baseUrl}/connect/token \
  -H ""Content-Type: application/x-www-form-urlencoded"" \
  -d ""grant_type=authorization_code&code={code}&redirect_uri={Uri.EscapeDataString($"{baseUrl}/api/test/test-callback")}&client_id=test-client&client_secret=test-secret""",
                    RequiredParameters = new
                    {
                        grant_type = "authorization_code",
                        code = code,
                        redirect_uri = $"{baseUrl}/api/test/test-callback",
                        client_id = "test-client",
                        client_secret = "test-secret"
                    }
                }
            });
        }

        /// <summary>
        /// Test token exchange
        /// </summary>
        [HttpPost("test-token-exchange")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> TestTokenExchange([FromForm] string code, [FromForm] string? state)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                
                using var client = new HttpClient();
                
                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", $"{baseUrl}/api/test/test-callback"),
                    new KeyValuePair<string, string>("client_id", "test-client"),
                    new KeyValuePair<string, string>("client_secret", "test-secret")
                });

                var response = await client.PostAsync($"{baseUrl}/connect/token", tokenRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                return Ok(new
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Response = responseContent,
                    Message = response.IsSuccessStatusCode ? "? Token exchange successful!" : "? Token exchange failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token exchange test failed");
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Check OpenIddict applications
        /// </summary>
        [HttpGet("openiddict-apps")]
        public async Task<IActionResult> GetOpenIddictApplications()
        {
            try
            {
                var applications = new List<object>();
                
                await foreach (var app in _applicationManager.ListAsync())
                {
                    var redirectUris = (await _applicationManager.GetRedirectUrisAsync(app)).ToArray();
                    var permissions = (await _applicationManager.GetPermissionsAsync(app)).ToArray();

                    applications.Add(new
                    {
                        Id = await _applicationManager.GetIdAsync(app),
                        ClientId = await _applicationManager.GetClientIdAsync(app),
                        DisplayName = await _applicationManager.GetDisplayNameAsync(app),
                        Type = await _applicationManager.GetClientTypeAsync(app),
                        ConsentType = await _applicationManager.GetConsentTypeAsync(app),
                        RedirectUris = redirectUris,
                        Permissions = permissions
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Applications = applications,
                    Count = applications.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get OpenIddict applications");
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get current authentication status
        /// </summary>
        [HttpGet("auth-status")]
        public async Task<IActionResult> GetAuthStatus()
        {
            try
            {
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                object userInfo = null;

                if (isAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        userInfo = new
                        {
                            Id = user.Id,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            EmailConfirmed = user.EmailConfirmed,
                            Roles = roles,
                            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray()
                        };
                    }
                }

                return Ok(new
                {
                    Success = true,
                    IsAuthenticated = isAuthenticated,
                    AuthenticationType = User.Identity?.AuthenticationType,
                    UserInfo = userInfo,
                    AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get auth status");
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Test email sending functionality
        /// </summary>
        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string email = "buihieudl@gmail.com")
        {
            try
            {
                var testLink = "https://localhost:7001/api/account/confirm-email?userId=test&token=test";
                await _emailService.SendEmailConfirmationAsync(email, testLink);
                
                return Ok(new { 
                    Success = true, 
                    Message = "Test email sent successfully",
                    Email = email 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test email failed");
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message 
                });
            }
        }

        private string GetSampleAuthorizationUrl(string baseUrl)
        {
            var redirectUri = Uri.EscapeDataString($"{baseUrl}/api/test/test-callback");
            return $"{baseUrl}/connect/authorize?" +
                   "client_id=test-client&" +
                   "response_type=code&" +
                   "scope=openid profile email roles&" +
                   $"redirect_uri={redirectUri}&" +
                   "state=test-state-12345";
        }
    }
}