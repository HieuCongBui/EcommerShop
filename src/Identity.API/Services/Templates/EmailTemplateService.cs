using Identity.API.Models.Templates;

namespace Identity.API.Services.Templates;

public class EmailTemplateService : IEmailTemplateService
{
    public string GetEmailConfirmationHtml(string confirmationLink)
    {
        var data = new EmailConfirmationTemplateData
        {
            ConfirmationLink = confirmationLink
        };

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirm Your Email</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
        <div style='background-color: #007bff; color: white; padding: 20px; border-radius: 10px 10px 0 0; margin: -30px -30px 20px -30px;'>
            <h1 style='margin: 0; font-size: 28px;'>{data.CompanyName}</h1>
        </div>
        
        <h2 style='color: #007bff; margin-bottom: 20px;'>Welcome to {data.CompanyName}!</h2>
        
        <p style='font-size: 16px; margin-bottom: 25px;'>
            Thank you for registering with us. To complete your registration and activate your account, 
            please confirm your email address by clicking the button below.
        </p>
        
        <a href='{data.ConfirmationLink}' 
           style='display: inline-block; background-color: #007bff; color: white; padding: 15px 30px; 
                  text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; margin: 20px 0;'>
            Confirm Email Address
        </a>
        
        <p style='font-size: 14px; color: #666; margin-top: 30px;'>
            If the button doesn't work, you can copy and paste this link into your browser:
        </p>
        
        <p style='word-break: break-all; background-color: #f1f1f1; padding: 10px; border-radius: 5px; font-size: 12px;'>
            {data.ConfirmationLink}
        </p>
        
        <p style='font-size: 12px; color: #999; margin-top: 30px;'>
            This link will expire in {data.ExpirationHours} hours. If you didn't create an account with us, please ignore this email.
        </p>
        
        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
        
        <p style='font-size: 12px; color: #999;'>
            © 2024 {data.CompanyName}. All rights reserved.
        </p>
    </div>
</body>
</html>";
    }

    public string GetEmailConfirmationPlainText(string confirmationLink)
    {
        var data = new EmailConfirmationTemplateData
        {
            ConfirmationLink = confirmationLink
        };

        return $@"
Welcome to {data.CompanyName}!

Thank you for registering with us. To complete your registration and activate your account, 
please confirm your email address by visiting the following link:

{data.ConfirmationLink}

This link will expire in {data.ExpirationHours} hours. If you didn't create an account with us, please ignore this email.

© 2024 {data.CompanyName}. All rights reserved.
";
    }

    public string GetPasswordResetHtml(string resetLink)
    {
        var data = new PasswordResetTemplateData
        {
            ResetLink = resetLink
        };

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
        <div style='background-color: #dc3545; color: white; padding: 20px; border-radius: 10px 10px 0 0; margin: -30px -30px 20px -30px;'>
            <h1 style='margin: 0; font-size: 28px;'>{data.CompanyName}</h1>
        </div>
        
        <h2 style='color: #dc3545; margin-bottom: 20px;'>Reset Your Password</h2>
        
        <p style='font-size: 16px; margin-bottom: 25px;'>
            We received a request to reset your password. If you made this request, 
            click the button below to set a new password.
        </p>
        
        <a href='{data.ResetLink}' 
           style='display: inline-block; background-color: #dc3545; color: white; padding: 15px 30px; 
                  text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; margin: 20px 0;'>
            Reset Password
        </a>
        
        <p style='font-size: 14px; color: #666; margin-top: 30px;'>
            If the button doesn't work, you can copy and paste this link into your browser:
        </p>
        
        <p style='word-break: break-all; background-color: #f1f1f1; padding: 10px; border-radius: 5px; font-size: 12px;'>
            {data.ResetLink}
        </p>
        
        <p style='font-size: 12px; color: #999; margin-top: 30px;'>
            This link will expire in {data.ExpirationMinutes} minutes. If you didn't request a password reset, please ignore this email.
        </p>
        
        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
        
        <p style='font-size: 12px; color: #999;'>
            © 2024 {data.CompanyName}. All rights reserved.
        </p>
    </div>
</body>
</html>";
    }

    public string GetPasswordResetPlainText(string resetLink)
    {
        var data = new PasswordResetTemplateData
        {
            ResetLink = resetLink
        };

        return $@"
Reset Your Password - {data.CompanyName}

We received a request to reset your password. If you made this request, 
visit the following link to set a new password:

{data.ResetLink}

This link will expire in {data.ExpirationMinutes} minutes. If you didn't request a password reset, please ignore this email.

© 2024 {data.CompanyName}. All rights reserved.
";
    }
}