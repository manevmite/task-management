using System.Net;
using System.Text.Json;
using TaskManagement.API.Models;

namespace TaskManagement.API.Middleware;

/// <summary>
/// Global error handling middleware
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. Request Path: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorResponse
        {
            Success = false,
            Message = "An error occurred while processing your request.",
            Path = context.Request.Path
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = exception.Message;
                errorResponse.StatusCode = HttpStatusCode.Unauthorized;
                break;

            case ArgumentNullException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = argEx.Message ?? "Required parameter is missing.";
                errorResponse.StatusCode = HttpStatusCode.BadRequest;
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = argEx.Message;
                errorResponse.StatusCode = HttpStatusCode.BadRequest;
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "The requested resource was not found.";
                errorResponse.StatusCode = HttpStatusCode.NotFound;
                break;

            case InvalidOperationException invalidOpEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = invalidOpEx.Message;
                errorResponse.StatusCode = HttpStatusCode.BadRequest;
                break;

            default:
                // Unhandled error
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = HttpStatusCode.InternalServerError;
                
                // Include exception details only in development
                if (_env.IsDevelopment())
                {
                    errorResponse.Message = exception.Message;
                    errorResponse.Details = exception.StackTrace;
                    errorResponse.InnerException = exception.InnerException?.Message;
                }
                else
                {
                    errorResponse.Message = "An internal server error occurred. Please try again later.";
                }
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, options);
        await response.WriteAsync(jsonResponse);
    }
}

