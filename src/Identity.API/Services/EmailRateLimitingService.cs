using Microsoft.Extensions.Caching.Memory;

namespace Identity.API.Services;

public class EmailRateLimitingService(IMemoryCache cache, ILogger<EmailRateLimitingService> logger) : IEmailRateLimitingService
{
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<EmailRateLimitingService> _logger = logger;
    private const int MaxEmailsPerHour = 5;
    private const int CacheExpirationHours = 1;

    public Task<bool> CanSendEmailAsync(string email)
    {
        var cacheKey = $"email_rate_limit_{email.ToLowerInvariant()}";
        
        if (_cache.TryGetValue(cacheKey, out List<DateTime>? timestamps))
        {
            // Remove timestamps older than 1 hour
            var oneHourAgo = DateTime.UtcNow.AddHours(-CacheExpirationHours);
            timestamps = timestamps?.Where(t => t > oneHourAgo).ToList() ?? [];
            
            if (timestamps.Count >= MaxEmailsPerHour)
            {
                _logger.LogWarning("Email rate limit exceeded for {Email}. Current count: {Count}", email, timestamps.Count);
                return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }

    public Task RecordEmailSentAsync(string email)
    {
        var cacheKey = $"email_rate_limit_{email.ToLowerInvariant()}";
        
        if (_cache.TryGetValue(cacheKey, out List<DateTime>? timestamps))
        {
            timestamps ??= [];
            
            // Remove timestamps older than 1 hour
            var oneHourAgo = DateTime.UtcNow.AddHours(-CacheExpirationHours);
            timestamps = [.. timestamps.Where(t => t > oneHourAgo)];
        }
        else
        {
            timestamps = [];
        }
        
        timestamps.Add(DateTime.UtcNow);
        
        _cache.Set(cacheKey, timestamps, TimeSpan.FromHours(CacheExpirationHours));
        
        _logger.LogInformation("Email sent recorded for {Email}. Total emails in last hour: {Count}", email, timestamps.Count);
        
        return Task.CompletedTask;
    }
}