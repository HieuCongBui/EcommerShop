namespace Identity.API.Models.Templates;

public class EmailConfirmationTemplateData
{
    public string ConfirmationLink { get; set; } = string.Empty;
    public string CompanyName { get; set; } = "EcommerShop";
    public string ExpirationHours { get; set; } = "24";
}

public class PasswordResetTemplateData
{
    public string ResetLink { get; set; } = string.Empty;
    public string CompanyName { get; set; } = "EcommerShop";
    public string ExpirationMinutes { get; set; } = "15";
}