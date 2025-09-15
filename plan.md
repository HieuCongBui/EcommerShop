# Implementation Plan: SendEmailConfirmationAsync in AccountController

## Overview
Implement email confirmation functionality using NETCore.MailKit.Core in the Identity.API service. The AccountController already references `IEmailService.SendEmailConfirmationAsync()` but the service implementation needs to be created.

## Prerequisites Analysis
- [x] AccountController already has `IEmailService` dependency injected
- [x] NETCore.MailKit.Core is already referenced in using statements
- [x] Email service is configured in Program.cs with DI registration
- [x] EmailService implementation created
- [x] EmailSettings configuration model created
- [x] Email templates/configuration implemented

## Implementation Steps

### Step 1: Verify and Create EmailSettings Configuration Model	
- [ ] Check if `EmailSettings` class exists in Models folder
- [ ] If not exists, create `EmailSettings.cs` with SMTP configuration properties
- [ ] Verify appsettings.json has EmailSettings section configured
- **Note: Need your confirmation on SMTP provider preferences (Gmail, SendGrid, etc.)**

### Step 2: Verify and Create IEmailService Interface
- [ ] Check if `IEmailService` interface exists in Services folder
- [ ] If not exists, create interface with required methods:
  - `SendEmailConfirmationAsync(string email, string confirmationLink)`
  - `SendPasswordResetAsync(string email, string resetLink)`
- [ ] Define proper method signatures and return types

### Step 3: Implement EmailService Class
- [ ] Create `EmailService.cs` class implementing `IEmailService`
- [ ] Implement `SendEmailConfirmationAsync` method using NETCore.MailKit.Core
- [ ] Implement `SendPasswordResetAsync` method for completeness
- [ ] Add proper error handling and logging
- [ ] Create email templates (HTML/plain text)

### Step 4: Update Configuration
- [ ] Verify Program.cs has proper DI registration for EmailService
- [ ] Add/verify EmailSettings configuration binding
- [ ] Ensure appsettings.json has proper email configuration structure

### Step 5: Create Email Templates
- [ ] Create HTML template for email confirmation
- [ ] Create fallback plain text template
- [ ] Implement template rendering with dynamic content (user name, confirmation link)
- **Note: Need your confirmation on email template design preferences**

### Step 6: Testing and Validation
- [ ] Test email service with development SMTP settings
- [ ] Verify email confirmation flow works end-to-end
- [ ] Add unit tests for EmailService methods
- [ ] Test error scenarios (invalid SMTP, network issues)

### Step 7: Security and Best Practices
- [ ] Implement rate limiting for email sending
- [ ] Add email validation and sanitization
- [ ] Ensure secure SMTP connection (TLS/SSL)
- [ ] Add logging for email operations

## Questions for Clarification

1. **SMTP Provider**: Which SMTP provider would you like to use? (Gmail, SendGrid, Outlook, Custom SMTP server)
[Answer: Gmail]
2. **Email Templates**: Do you have specific branding or template requirements for the confirmation emails?
[Answer: Simple HTML with logo and link]]
3. **Configuration**: Should email settings be environment-specific (dev/staging/prod)?
[Answer: Yes, use appsettings.{Environment}.json]]
4. **Error Handling**: How should email failures be handled? (Silent fail, retry mechanism, admin notification)
[Answer: Retry mechanism with logging]]]
5. **Rate Limiting**: Do you want to implement rate limiting for email sending per user?
[Answer: Yes, limit to 5 emails per hour per user]