using System.Net;
using System.Text.Json;
using Contracts;
using Models.Errors;
using FluentValidationException = FluentValidation.ValidationException;

namespace Middlewares
{

    /// <summary>
    /// Middleware that handles unhandled exceptions, logs them, and returns standardized error responses.
    /// Also measures and logs request duration.
    /// </summary>
    internal class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> _logger)
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            { 
                await next(httpContext);
            }
            catch (NotFoundException e)
            {
                await SetResponse(e, httpContext, HttpStatusCode.NotFound);
            }
            catch (ConflictException e)
            {
                await SetResponse(e, httpContext, HttpStatusCode.Conflict);
            }
            catch (ValidationException e)
            {
                await SetResponse(e, httpContext, HttpStatusCode.BadRequest);
            }
            catch (FluentValidationException e)
            {
                await SetValidationResponse(e, httpContext);
            }
            catch(Exception e)
            {
                await SetResponse(e, httpContext, HttpStatusCode.InternalServerError);
            }
        }

         private async Task SetResponse(Exception e, HttpContext httpContext, HttpStatusCode code)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot write error details");
                return;
            }

            var response = e is ResourceException resourceException
                ? new ErrorResponse(
                    Title: code.ToString(),
                    Detail: GetMessage(resourceException, code),
                    ResourceType: resourceException.ResourceType,
                    ResourceId: resourceException.ResourceId
                )
                : new ErrorResponse(
                    Title: code.ToString(),
                    Detail: e.Message
                );

            httpContext.Response.StatusCode = (int)code;
            httpContext.Response.ContentType = "application/json";

            var content = JsonSerializer.Serialize(response, _jsonOptions);
            await httpContext.Response.WriteAsync(content);
        }

        private async Task SetValidationResponse(FluentValidationException e, HttpContext httpContext)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot write error details");
                return;
            }

            var errors = e.Errors
                .GroupBy(f => f.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

            var response = new ErrorResponse(
                Title: "BadRequest",
                Detail: "One or more validation errors occurred",
                Errors: errors
            );

            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            httpContext.Response.ContentType = "application/json";

            var content = JsonSerializer.Serialize(response, _jsonOptions);
            await httpContext.Response.WriteAsync(content);
        }

        private string GetMessage(ResourceException e, HttpStatusCode code)
        {
            return code switch
            {
                HttpStatusCode.NotFound => !string.IsNullOrEmpty(e.Message) ? e.Message : $"{e.ResourceType} not found",
                HttpStatusCode.Conflict => !string.IsNullOrEmpty(e.Message) ? e.Message : $"{e.ResourceType} conflict occurred",
                HttpStatusCode.Unauthorized => !string.IsNullOrEmpty(e.Message) ? e.Message : "Unauthorized access",
                HttpStatusCode.Forbidden => !string.IsNullOrEmpty(e.Message) ? e.Message : "Access forbidden",
                HttpStatusCode.BadRequest => !string.IsNullOrEmpty(e.Message) ? e.Message : "Invalid request",
                _ => !string.IsNullOrEmpty(e.Message) ? e.Message : "An error occurred"
            };
        }

    }   
}