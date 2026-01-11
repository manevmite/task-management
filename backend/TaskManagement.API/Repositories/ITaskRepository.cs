using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

/// <summary>
/// Repository interface for task operations
/// </summary>
public interface ITaskRepository
{
    System.Threading.Tasks.Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(int userId, bool? isCompleted = null);
    System.Threading.Tasks.Task<TaskItem?> GetByIdAsync(int id, int userId);
    System.Threading.Tasks.Task<TaskItem> CreateAsync(TaskItem task);
    System.Threading.Tasks.Task<TaskItem> UpdateAsync(TaskItem task);
    System.Threading.Tasks.Task<bool> DeleteAsync(int id, int userId);
}

