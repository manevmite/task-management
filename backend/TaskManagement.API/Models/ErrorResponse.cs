using System.Net;

namespace TaskManagement.API.Models;

/// <summary>
/// Standard error response model
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error information (only in development)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Inner exception message (only in development)
    /// </summary>
    public string? InnerException { get; set; }

    /// <summary>
    /// Validation errors (if any)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Timestamp of the error
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request path that caused the error
    /// </summary>
    public string? Path { get; set; }
}

