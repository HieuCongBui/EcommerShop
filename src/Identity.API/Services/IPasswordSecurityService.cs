using Identity.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Services;

public interface IPasswordSecurityService
{
    Task<bool> IsPasswordUsedRecentlyAsync(ApplicationUser user, string password);
    Task SavePasswordHistoryAsync(ApplicationUser user, string passwordHash);
    Task<PasswordValidationResult> ValidatePasswordStrengthAsync(string password);
    Task CleanupOldPasswordHistoryAsync(ApplicationUser user);
}

public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public PasswordStrengthLevel StrengthLevel { get; set; }
    public int Score { get; set; }
}

public enum PasswordStrengthLevel
{
    VeryWeak = 1,
    Weak = 2,
    Fair = 3,
    Good = 4,
    Strong = 5
}