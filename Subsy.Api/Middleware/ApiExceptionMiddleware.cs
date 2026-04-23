using System.Text.Json;

namespace Subsy.Api.Middleware;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ApiExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = ex switch
            {
                KeyNotFoundException => (404, "Resource not found."),
                UnauthorizedAccessException => (403, "Access denied."),
                InvalidOperationException => (400, ex.Message),
                FluentValidation.ValidationException vex => (400,
                    string.Join("; ", vex.Errors.Select(e => e.ErrorMessage))),
                _ => (500, "An unexpected error occurred.")
            };

            context.Response.StatusCode = statusCode;

            var response = JsonSerializer.Serialize(new
            {
                status = statusCode,
                message
            });

            await context.Response.WriteAsync(response);
        }
    }
}