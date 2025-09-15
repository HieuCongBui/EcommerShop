using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models;

public class PasswordHistory
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual ApplicationUser User { get; set; } = null!;
}