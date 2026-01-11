using TaskManagement.API.DTOs.Tasks;

namespace TaskManagement.API.Services;

/// <summary>
/// Service interface for task operations
/// </summary>
public interface ITaskService
{
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId, bool? isCompleted = null);
    System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(int id, int userId);
    System.Threading.Tasks.Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, int userId);
    System.Threading.Tasks.Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskRequest request, int userId);
    System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id, int userId);
}

