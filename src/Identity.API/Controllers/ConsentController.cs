using Identity.API.Models.DTOs;
using Identity.API.Models.ViewModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace Identity.API.Controllers;

[Route("consent")]
[Authorize]
public class ConsentController(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager) : ControllerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager = applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager = scopeManager;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AuthorizeViewModel>>> GetConsentForm([FromQuery] string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            return BadRequest(ApiResponse<AuthorizeViewModel>.ErrorResult("Return URL is required"));
        }

        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest(ApiResponse<AuthorizeViewModel>.ErrorResult("Invalid OAuth request"));
        }

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
        if (application == null)
        {
            return BadRequest(ApiResponse<AuthorizeViewModel>.ErrorResult("Invalid client application"));
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
        }

        var viewModel = new AuthorizeViewModel
        {
            ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application),
            Scope = string.Join(" ", scopes),
            Scopes = scopes.ToList(),
            ScopeDescriptions = scopeDescriptions
        };

        return Ok(ApiResponse<AuthorizeViewModel>.SuccessResult(viewModel, "Consent form retrieved successfully"));
    }

    private static string GetDefaultScopeDescription(string scopeName)
    {
        return scopeName switch
        {
            "openid" => "Access to your unique identifier",
            "profile" => "Access to your profile information (name, etc.)",
            "email" => "Access to your email address",
            "catalog" => "Access to catalog information",
            "roles" => "Access to your role information",
            _ => $"Access to {scopeName} resources"
        };
    }
}