namespace TaskManagement.Application.DTOs.Auth;

/// <summary>
/// Response DTO for authentication operations
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

