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
        try
        {
            // Validate inputs
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email address cannot be null or empty", nameof(email));
            
            if (string.IsNullOrEmpty(confirmationLink))
                throw new ArgumentException("Confirmation link cannot be null or empty", nameof(confirmationLink));

            // Check rate limiting
            if (!await _rateLimitingService.CanSendEmailAsync(email))
            {
                _logger.LogWarning("Email rate limit exceeded for {Email}", email);
                throw new InvalidOperationException("Email rate limit exceeded. Please try again later.");
            }

            // Validate email settings
            if (string.IsNullOrEmpty(_emailSettings.SmtpServer) || 
                string.IsNullOrEmpty(_emailSettings.Username) || 
                string.IsNullOrEmpty(_emailSettings.Password))
            {
                _logger.LogError("Email settings are not properly configured");
                throw new InvalidOperationException("Email service is not properly configured");
            }

            var subject = "Confirm Your Email Address";
            var htmlBody = _templateService.GetEmailConfirmationHtml(confirmationLink);
            var plainTextBody = _templateService.GetEmailConfirmationPlainText(confirmationLink);

            // Validate templates
            if (string.IsNullOrEmpty(htmlBody) || string.IsNullOrEmpty(plainTextBody))
            {
                _logger.LogError("Email templates could not be generated");
                throw new InvalidOperationException("Email templates are not available");
            }

            await SendEmailWithRetryAsync(email, subject, htmlBody, plainTextBody);
            await _rateLimitingService.RecordEmailSentAsync(email);
            
            _logger.LogInformation("Email confirmation sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email confirmation to {Email}", email);
            throw;
        }
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
                _logger.LogDebug("Attempting to send email to {Email}, attempt {Attempt}", email, retryCount + 1);
                
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                client.EnableSsl = _emailSettings.EnableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

                // Add timeout
                client.Timeout = 30000; // 30 seconds

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

                _logger.LogDebug("Connecting to SMTP server {Server}:{Port} with SSL={EnableSsl}", 
                    _emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl);

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {Email} on attempt {Attempt}", email, retryCount + 1);
                return;
            }
            catch (SmtpException smtpEx)
            {
                retryCount++;
                _logger.LogWarning(smtpEx, "SMTP error sending email to {Email}. Status: {StatusCode}. Attempt {Attempt} of {MaxRetries}", 
                    email, smtpEx.StatusCode, retryCount, maxRetries);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError(smtpEx, "Failed to send email to {Email} after {MaxRetries} attempts. SMTP Status: {StatusCode}", 
                        email, maxRetries, smtpEx.StatusCode);
                    throw new InvalidOperationException($"Failed to send email after {maxRetries} attempts. SMTP Error: {smtpEx.Message}", smtpEx);
                }

                // Don't retry for certain SMTP errors
                if (smtpEx.StatusCode == SmtpStatusCode.MailboxBusy || 
                    smtpEx.StatusCode == SmtpStatusCode.InsufficientStorage ||
                    smtpEx.StatusCode == SmtpStatusCode.MailboxUnavailable)
                {
                    _logger.LogError(smtpEx, "Non-retryable SMTP error for {Email}: {StatusCode}", email, smtpEx.StatusCode);
                    throw;
                }
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to send email to {Email}. Attempt {Attempt} of {MaxRetries}", 
                    email, retryCount, maxRetries);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "Failed to send email to {Email} after {MaxRetries} attempts", email, maxRetries);
                    throw new InvalidOperationException($"Failed to send email after {maxRetries} attempts: {ex.Message}", ex);
                }
            }

            // Exponential backoff
            var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, retryCount - 1));
            _logger.LogDebug("Waiting {DelayMs}ms before retry {RetryCount}", delay.TotalMilliseconds, retryCount);
            await Task.Delay(delay);
        }
    }
}