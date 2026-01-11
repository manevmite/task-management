using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
/// Repository interface specifically for Task operations
/// </summary>
public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(int userId, bool? isCompleted = null);
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetAllByUserIdPagedAsync(
        int userId, 
        bool? isCompleted = null, 
        int page = 1, 
        int pageSize = 5);
    Task<TaskItem?> GetByIdAsync(int id, int userId);
    Task<bool> DeleteAsync(int id, int userId);
}

