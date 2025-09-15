using Identity.API.Models;
using Microsoft.Extensions.Options;
using NETCore.MailKit.Core;
using System.Net;
using System.Net.Mail;
using Identity.API.Services.Templates;

namespace Identity.API.Services;

public class EmailService(
    IOptions<EmailSettings> emailSettings,
    IEmailRateLimitingService rateLimitingService,
    ILogger<EmailService> logger,
    IEmailTemplateService templateService) : IEmailService   
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;
    private readonly IEmailRateLimitingService _rateLimitingService = rateLimitingService;
    private readonly ILogger<EmailService> _logger = logger;
    private readonly IEmailTemplateService _templateService = templateService;

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        // Check rate limiting
        if (!await _rateLimitingService.CanSendEmailAsync(email))
        {
            _logger.LogWarning("Email rate limit exceeded for {Email}", email);
            throw new InvalidOperationException("Email rate limit exceeded. Please try again later.");
        }

        var subject = "Confirm Your Email Address";
        var htmlBody = _templateService.GetEmailConfirmationHtml(confirmationLink);
        var plainTextBody = _templateService.GetEmailConfirmationPlainText(confirmationLink);

        await SendEmailWithRetryAsync(email, subject, htmlBody, plainTextBody);
        await _rateLimitingService.RecordEmailSentAsync(email);
    }

    public async Task SendPasswordResetAsync(string email, string resetLink)
    {
        // Check rate limiting
        if (!await _rateLimitingService.CanSendEmailAsync(email))
        {
            _logger.LogWarning("Email rate limit exceeded for {Email}", email);
            throw new InvalidOperationException("Email rate limit exceeded. Please try again later.");
        }

        var subject = "Reset Your Password";
        var htmlBody = _templateService.GetPasswordResetHtml(resetLink);        
        var plainTextBody = _templateService.GetPasswordResetPlainText(resetLink); 

        await SendEmailWithRetryAsync(email, subject, htmlBody, plainTextBody);
        await _rateLimitingService.RecordEmailSentAsync(email);
    }

    private async Task SendEmailWithRetryAsync(string email, string subject, string htmlBody, string plainTextBody, int maxRetries = 3)
    {
        var retryCount = 0;
        var baseDelay = TimeSpan.FromSeconds(1);

        while (retryCount < maxRetries)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                client.EnableSsl = _emailSettings.EnableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

                using var message = new MailMessage();
                message.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                message.To.Add(email);
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;

                // Add plain text alternative
                var plainTextView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
                message.AlternateViews.Add(plainTextView);
                message.AlternateViews.Add(htmlView);

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {Email}", email);
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to send email to {Email}. Attempt {Attempt} of {MaxRetries}", 
                    email, retryCount, maxRetries);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "Failed to send email to {Email} after {MaxRetries} attempts", email, maxRetries);
                    throw new InvalidOperationException($"Failed to send email after {maxRetries} attempts", ex);
                }

                // Exponential backoff
                var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, retryCount - 1));
                await Task.Delay(delay);
            }
        }
    }
}