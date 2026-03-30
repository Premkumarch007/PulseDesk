namespace PulseDesk.Shared.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public List<string> Policies { get; set; } = new();
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}