using Identity.API.Data;
using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Identity.API.Services;

public class PasswordSecurityService : IPasswordSecurityService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly ILogger<PasswordSecurityService> _logger;
    private const int MaxPasswordHistory = 5; // Remember last 5 passwords
    private const int PasswordHistoryDays = 90; // Keep history for 90 days

    public PasswordSecurityService(
        ApplicationDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        ILogger<PasswordSecurityService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<bool> IsPasswordUsedRecentlyAsync(ApplicationUser user, string password)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-PasswordHistoryDays);
            var recentPasswords = await _context.Set<PasswordHistory>()
                .Where(ph => ph.UserId == user.Id && ph.CreatedAt >= cutoffDate)
                .OrderByDescending(ph => ph.CreatedAt)
                .Take(MaxPasswordHistory)
                .ToListAsync();

            foreach (var passwordHistory in recentPasswords)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, passwordHistory.PasswordHash, password);
                if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking password history for user {UserId}", user.Id);
            return false;
        }
    }

    public async Task SavePasswordHistoryAsync(ApplicationUser user, string passwordHash)
    {
        try
        {
            var passwordHistory = new PasswordHistory
            {
                UserId = user.Id,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<PasswordHistory>().Add(passwordHistory);
            await _context.SaveChangesAsync();

            // Clean up old history
            await CleanupOldPasswordHistoryAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving password history for user {UserId}", user.Id);
        }
    }

    public Task<PasswordValidationResult> ValidatePasswordStrengthAsync(string password)
    {
        var result = new PasswordValidationResult();
        var errors = new List<string>();
        var score = 0;

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Password is required");
            result.IsValid = false;
            result.Errors = errors;
            result.StrengthLevel = PasswordStrengthLevel.VeryWeak;
            return Task.FromResult(result);
        }

        // Length check
        if (password.Length >= 8) score += 1;
        else errors.Add("Password must be at least 8 characters long");

        if (password.Length >= 12) score += 1;

        // Character diversity checks
        if (Regex.IsMatch(password, @"[a-z]")) score += 1;
        else errors.Add("Password must contain at least one lowercase letter");

        if (Regex.IsMatch(password, @"[A-Z]")) score += 1;
        else errors.Add("Password must contain at least one uppercase letter");

        if (Regex.IsMatch(password, @"[0-9]")) score += 1;
        else errors.Add("Password must contain at least one digit");

        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) score += 1;
        else errors.Add("Password must contain at least one special character");

        // Additional strength checks
        if (password.Length >= 16) score += 1;
        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?].*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) score += 1; // Multiple special chars

        // Check for common patterns (reduce score)
        if (HasCommonPatterns(password)) score -= 2;
        if (IsCommonPassword(password)) score -= 3;

        score = Math.Max(0, Math.Min(5, score)); // Clamp between 0 and 5

        result.Score = score;
        result.StrengthLevel = (PasswordStrengthLevel)Math.Max(1, score);
        result.IsValid = score >= 4 && !errors.Any(); // Require "Good" strength and no basic errors
        result.Errors = errors;

        return Task.FromResult(result);
    }

    public async Task CleanupOldPasswordHistoryAsync(ApplicationUser user)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-PasswordHistoryDays);
            var oldPasswords = await _context.Set<PasswordHistory>()
                .Where(ph => ph.UserId == user.Id && ph.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldPasswords.Any())
            {
                _context.Set<PasswordHistory>().RemoveRange(oldPasswords);
                await _context.SaveChangesAsync();
            }

            // Keep only the most recent passwords within the limit
            var excessPasswords = await _context.Set<PasswordHistory>()
                .Where(ph => ph.UserId == user.Id)
                .OrderByDescending(ph => ph.CreatedAt)
                .Skip(MaxPasswordHistory)
                .ToListAsync();

            if (excessPasswords.Any())
            {
                _context.Set<PasswordHistory>().RemoveRange(excessPasswords);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up password history for user {UserId}", user.Id);
        }
    }

    private static bool HasCommonPatterns(string password)
    {
        // Check for keyboard patterns
        var keyboardPatterns = new[]
        {
            "qwerty", "asdf", "zxcv", "123456", "abcdef",
            "qwertyuiop", "asdfghjkl", "zxcvbnm"
        };

        var lowerPassword = password.ToLower();
        return keyboardPatterns.Any(pattern => lowerPassword.Contains(pattern));
    }

    private static bool IsCommonPassword(string password)
    {
        // List of most common passwords
        var commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password", "123456", "123456789", "12345678", "12345",
            "1234567", "password1", "123123", "admin", "qwerty",
            "abc123", "Password1", "welcome", "monkey", "dragon",
            "master", "hello", "letmein", "login", "princess"
        };

        return commonPasswords.Contains(password);
    }
}