namespace Identity.API.Models.DTOs;

public class PasswordStrengthDto
{
    public bool IsValid { get; set; }
    public int Score { get; set; }
    public string StrengthLevel { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}