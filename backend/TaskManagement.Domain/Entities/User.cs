namespace TaskManagement.Domain.Entities;

/// <summary>
/// Represents a user entity in the domain
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public List<TaskItem> Tasks { get; set; } = new();
}

