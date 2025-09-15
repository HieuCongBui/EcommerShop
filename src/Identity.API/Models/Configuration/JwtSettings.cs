namespace Identity.API.Models.Configuration;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public int AuthorizationCodeExpirationMinutes { get; set; } = 10;
    public int IdentityTokenExpirationMinutes { get; set; } = 60;
}