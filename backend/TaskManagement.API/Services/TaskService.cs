using TaskManagement.API.DTOs.Tasks;
using TaskManagement.API.Models;
using TaskManagement.API.Repositories;

namespace TaskManagement.API.Services;

/// <summary>
/// Service implementation for task business logic
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId, bool? isCompleted = null)
    {
        var tasks = await _taskRepository.GetAllByUserIdAsync(userId, isCompleted);
        return tasks.Select(MapToDto);
    }

    public async System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(int id, int userId)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId);
        return task == null ? null : MapToDto(task);
    }

    public async System.Threading.Tasks.Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, int userId)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            UserId = userId
        };

        var createdTask = await _taskRepository.CreateAsync(task);
        return MapToDto(createdTask);
    }

    public async System.Threading.Tasks.Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskRequest request, int userId)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId);
        if (task == null)
        {
            return null;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;

        var updatedTask = await _taskRepository.UpdateAsync(task);
        return MapToDto(updatedTask);
    }

    public async System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id, int userId)
    {
        return await _taskRepository.DeleteAsync(id, userId);
    }

    private static TaskDto MapToDto(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}

