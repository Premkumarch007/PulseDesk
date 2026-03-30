// PulseDesk.Domain/Entities/AuditLog.cs
namespace PulseDesk.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = default!;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User? User { get; set; }
}