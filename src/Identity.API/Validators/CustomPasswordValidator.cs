using Identity.API.Models;
using Identity.API.Services;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Validators;

public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
{
    private readonly IPasswordSecurityService _passwordSecurityService;

    public CustomPasswordValidator(IPasswordSecurityService passwordSecurityService)
    {
        _passwordSecurityService = passwordSecurityService;
    }

    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        var errors = new List<IdentityError>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequired",
                Description = "Password is required."
            });
            return IdentityResult.Failed(errors.ToArray());
        }

        // Check password strength
        var strengthResult = await _passwordSecurityService.ValidatePasswordStrengthAsync(password);
        if (!strengthResult.IsValid)
        {
            foreach (var error in strengthResult.Errors)
            {
                errors.Add(new IdentityError
                {
                    Code = "WeakPassword",
                    Description = error
                });
            }
        }

        // Check if password was used recently
        if (user != null && await _passwordSecurityService.IsPasswordUsedRecentlyAsync(user, password))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRecentlyUsed",
                Description = "This password was used recently. Please choose a different password."
            });
        }

        // Check if password contains user information
        if (user != null && ContainsUserInformation(password, user))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordContainsUserInfo",
                Description = "Password cannot contain your name, email, or username."
            });
        }

        return errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
    }

    private static bool ContainsUserInformation(string password, ApplicationUser user)
    {
        var lowerPassword = password.ToLower();
        
        if (!string.IsNullOrEmpty(user.Email) && lowerPassword.Contains(user.Email.Split('@')[0].ToLower()))
            return true;
            
        if (!string.IsNullOrEmpty(user.UserName) && lowerPassword.Contains(user.UserName.ToLower()))
            return true;
            
        if (!string.IsNullOrEmpty(user.FirstName) && user.FirstName.Length > 2 && lowerPassword.Contains(user.FirstName.ToLower()))
            return true;
            
        if (!string.IsNullOrEmpty(user.LastName) && user.LastName.Length > 2 && lowerPassword.Contains(user.LastName.ToLower()))
            return true;

        return false;
    }
}