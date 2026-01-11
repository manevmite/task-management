using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

/// <summary>
/// Repository implementation for task data access
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext _context;

    public TaskRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(int userId, bool? isCompleted = null)
    {
        var query = _context.Tasks.Where(t => t.UserId == userId);

        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async System.Threading.Tasks.Task<TaskItem?> GetByIdAsync(int id, int userId)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async System.Threading.Tasks.Task<TaskItem> CreateAsync(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async System.Threading.Tasks.Task<TaskItem> UpdateAsync(TaskItem task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id, int userId)
    {
        var task = await GetByIdAsync(id, userId);
        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}

