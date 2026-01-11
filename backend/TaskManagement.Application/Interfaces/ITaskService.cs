using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Tasks;

namespace TaskManagement.Application.Interfaces;

/// <summary>
/// Service interface for task operations
/// </summary>
public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId, bool? isCompleted = null);
    Task<PaginatedResponse<TaskDto>> GetTasksByUserIdPagedAsync(
        int userId, 
        bool? isCompleted = null, 
        int page = 1, 
        int pageSize = 5);
    Task<TaskDto?> GetTaskByIdAsync(int id, int userId);
    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, int userId);
    Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskRequest request, int userId);
    Task<bool> DeleteTaskAsync(int id, int userId);
}

