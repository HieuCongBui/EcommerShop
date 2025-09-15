using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}