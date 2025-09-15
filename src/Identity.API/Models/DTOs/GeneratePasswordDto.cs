using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.DTOs;

public class GeneratePasswordDto
{
    [Range(8, 128)]
    public int Length { get; set; } = 12;
    
    public bool IncludeSymbols { get; set; } = true;
    
    public bool ExcludeSimilar { get; set; } = true;
}