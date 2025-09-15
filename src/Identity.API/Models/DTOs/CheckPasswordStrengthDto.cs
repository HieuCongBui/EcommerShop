using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.DTOs;

public class CheckPasswordStrengthDto
{
    [Required]
    public string Password { get; set; } = string.Empty;
}