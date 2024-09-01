using api.Shared.Models.Errors;
using System.Net;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly Dictionary<Type, Func<Exception, HttpContext, Task>> _exceptionHandlers;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _exceptionHandlers = new Dictionary<Type, Func<Exception, HttpContext, Task>>
        {
            { typeof(RoomAlreadyExistsException) , HandleValidationException },
            { typeof(NotFoundException) , HandleValidationException },
            { typeof(ServiceValidationException), HandleValidationException },
            { typeof(Exception), HandleGeneralException }
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (_exceptionHandlers.TryGetValue(ex.GetType(), out var handler))
            {
                await handler(ex, context);
            }
            else
            {
                await HandleGeneralException(ex, context);
            }
        }
    }

    private Task HandleValidationException(Exception ex, HttpContext context)
    {
        _logger.LogInformation(ex, "A validation exception occurred.");
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        var response = new { error = ex.Message };
        return context.Response.WriteAsJsonAsync(response);
    }

    private Task HandleGeneralException(Exception ex, HttpContext context)
    {
        _logger.LogError(ex, "An unhandled exception occurred.");
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var response = new { error = "An error occurred while processing your request." };
        return context.Response.WriteAsJsonAsync(response);
    }
}
