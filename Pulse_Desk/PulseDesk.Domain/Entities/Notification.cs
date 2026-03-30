// PulseDesk.Domain/Entities/Notification.cs
namespace PulseDesk.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Type { get; set; } = "info";
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User User { get; set; } = default!;
}