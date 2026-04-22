using System.Net;
using System.Text.Json;

namespace AutiCare.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        (int statusCode, string message) = ex switch
        {
            KeyNotFoundException       => (404, ex.Message),
            UnauthorizedAccessException => (401, ex.Message),
            InvalidOperationException or ArgumentException when ex.Message.Contains("Conflict") => (409, ex.Message),
            InvalidOperationException  => (400, ex.Message),
            ArgumentException          => (400, ex.Message),
            _                          => (500, "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        
        var response = new
        {
            success = false,
            message = message,
            errors = new string[] { }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
