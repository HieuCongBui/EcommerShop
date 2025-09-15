using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.DTOs;

public class CreateRoleDto
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}