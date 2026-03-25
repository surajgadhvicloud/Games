using FluentValidation;

namespace BoardGamesLibrary.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing request.");
            await WriteErrorAsync(context, ex);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Business rule violation"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            title,
            status = statusCode,
            detail = exception.Message
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}