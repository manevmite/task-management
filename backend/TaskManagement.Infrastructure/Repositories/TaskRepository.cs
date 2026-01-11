using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for task data access
/// </summary>
public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(int userId, bool? isCompleted = null)
    {
        var query = _context.Tasks.Where(t => t.UserId == userId);

        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetAllByUserIdPagedAsync(
        int userId, 
        bool? isCompleted = null, 
        int page = 1, 
        int pageSize = 5)
    {
        var query = _context.Tasks.Where(t => t.UserId == userId);

        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<TaskItem?> GetByIdAsync(int id, int userId)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public new async Task<TaskItem> AddAsync(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        return await base.AddAsync(task);
    }

    public new async Task UpdateAsync(TaskItem task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        await base.UpdateAsync(task);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var task = await GetByIdAsync(id, userId);
        if (task == null)
        {
            return false;
        }

        await base.DeleteAsync(task);
        return true;
    }
}

