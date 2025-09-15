namespace Identity.API.Services.Templates;

public interface IEmailTemplateService
{
    string GetEmailConfirmationHtml(string confirmationLink);
    string GetEmailConfirmationPlainText(string confirmationLink);
    string GetPasswordResetHtml(string resetLink);
    string GetPasswordResetPlainText(string resetLink);
}