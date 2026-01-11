namespace TaskManagement.Domain.Entities;

/// <summary>
/// Represents a task entity in the domain
/// </summary>
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}

