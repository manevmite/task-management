using TaskManagement.API.DTOs.Auth;

namespace TaskManagement.API.Services;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthService
{
    System.Threading.Tasks.Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    System.Threading.Tasks.Task<AuthResponse?> LoginAsync(LoginRequest request);
    int GetUserIdFromToken(string token);
}

