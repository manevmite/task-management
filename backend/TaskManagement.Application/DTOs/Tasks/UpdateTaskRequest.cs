using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs.Tasks;

/// <summary>
/// Request DTO for updating an existing task
/// </summary>
public class UpdateTaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }
}

