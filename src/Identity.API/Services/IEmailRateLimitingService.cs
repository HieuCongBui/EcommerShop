namespace Identity.API.Services;

public interface IEmailRateLimitingService
{
    Task<bool> CanSendEmailAsync(string email);
    Task RecordEmailSentAsync(string email);
}