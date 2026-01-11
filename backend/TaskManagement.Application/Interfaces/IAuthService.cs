using TaskManagement.Application.DTOs.Auth;

namespace TaskManagement.Application.Interfaces;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    int GetUserIdFromToken(string token);
}

