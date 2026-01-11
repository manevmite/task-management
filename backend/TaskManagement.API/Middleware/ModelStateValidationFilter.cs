using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Text.Json;
using TaskManagement.API.Models;

namespace TaskManagement.API.Middleware;

/// <summary>
/// Action filter to handle model validation errors
/// </summary>
public class ModelStateValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new Dictionary<string, string[]>();

            foreach (var modelState in context.ModelState)
            {
                var key = modelState.Key;
                var errorMessages = modelState.Value.Errors
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage)
                    .ToArray();

                if (errorMessages.Length > 0)
                {
                    errors[key] = errorMessages;
                }
            }

            var errorResponse = new ErrorResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Validation failed. Please check the errors and try again.",
                Errors = errors,
                Path = context.HttpContext.Request.Path,
                Timestamp = DateTime.UtcNow
            };

            context.Result = new BadRequestObjectResult(errorResponse);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Not needed
    }
}

