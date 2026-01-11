using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Services;

/// <summary>
/// Service implementation for task business logic
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ITaskRepository taskRepository, ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId, bool? isCompleted = null)
    {
        var tasks = await _taskRepository.GetAllByUserIdAsync(userId, isCompleted);
        return tasks.Select(MapToDto);
    }

    public async Task<PaginatedResponse<TaskDto>> GetTasksByUserIdPagedAsync(
        int userId, 
        bool? isCompleted = null, 
        int page = 1, 
        int pageSize = 5)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 5;

        var (items, totalCount) = await _taskRepository.GetAllByUserIdPagedAsync(
            userId, 
            isCompleted, 
            page, 
            pageSize);

        return new PaginatedResponse<TaskDto>
        {
            Items = items.Select(MapToDto),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int id, int userId)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId);
        return task == null ? null : MapToDto(task);
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, int userId)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            UserId = userId
        };

        var createdTask = await _taskRepository.AddAsync(task);
        _logger.LogInformation("Task created: {TaskId} by user {UserId}", createdTask.Id, userId);
        return MapToDto(createdTask);
    }

    public async Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskRequest request, int userId)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId);
        if (task == null)
        {
            _logger.LogWarning("Task not found for update: {TaskId} by user {UserId}", id, userId);
            return null;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;

        await _taskRepository.UpdateAsync(task);
        _logger.LogInformation("Task updated: {TaskId} by user {UserId}", id, userId);
        return MapToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(int id, int userId)
    {
        var result = await _taskRepository.DeleteAsync(id, userId);
        if (result)
        {
            _logger.LogInformation("Task deleted: {TaskId} by user {UserId}", id, userId);
        }
        else
        {
            _logger.LogWarning("Task not found for deletion: {TaskId} by user {UserId}", id, userId);
        }
        return result;
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

