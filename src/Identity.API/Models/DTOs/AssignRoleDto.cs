using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.DTOs;

public class AssignRoleDto
{
    [Required]
    public string RoleName { get; set; } = string.Empty;
}