namespace Identity.API.Models.DTOs;

public class EmailConfirmationStatusDto
{
    public bool IsConfirmed { get; set; }
    public DateTime? EmailSentAt { get; set; }
    public DateTime? EmailClickedAt { get; set; }
    public DateTime? EmailConfirmedAt { get; set; }
    public int AttemptCount { get; set; }
    public bool HasClickedLink { get; set; }
}