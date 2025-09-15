using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.DTOs;

public class ConsentDto
{
    [Required]
    public string ReturnUrl { get; set; } = string.Empty;
    
    public List<string> ScopesToConsent { get; set; } = new();
    
    public bool RememberConsent { get; set; }
    
    public string Action { get; set; } = string.Empty; // "allow" or "deny"
}