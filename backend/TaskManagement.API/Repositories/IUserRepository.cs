using TaskManagement.API.Models;

namespace TaskManagement.API.Repositories;

/// <summary>
/// Repository interface for user operations
/// </summary>
public interface IUserRepository
{
    System.Threading.Tasks.Task<User?> GetByEmailAsync(string email);
    System.Threading.Tasks.Task<User?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<bool> UserExistsAsync(string email);
    System.Threading.Tasks.Task<User> CreateAsync(User user);
}

