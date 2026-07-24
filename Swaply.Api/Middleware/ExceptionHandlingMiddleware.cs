using System.Net;
using System.Text.Json;
using Swaply.Domain.Exceptions;

namespace Swaply.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred during the request. Path: {Path}, Method: {Method}", 
                context.Request.Path, context.Request.Method);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var message = string.IsNullOrWhiteSpace(exception.Message) ? "An error occurred while processing your request." : exception.Message;
        var extraData = (object?)null;

        // Check DuplicateExchangeException BEFORE DomainException
        // because DuplicateExchangeException inherits from DomainException
        if (exception is DuplicateExchangeException dupEx)
        {
            code = HttpStatusCode.Conflict;
            message = dupEx.Message;
            if (dupEx.ExistingExchangeId.HasValue)
            {
                extraData = new { existingExchangeId = dupEx.ExistingExchangeId.Value };
            }
        }
        else if (exception is DomainException domainEx)
        {
            code = HttpStatusCode.BadRequest;
            message = domainEx.Message;
        }
        else if (exception is UnauthorizedAccessException)
        {
            code = HttpStatusCode.Forbidden;
            message = exception.Message;
        }
        else if (exception is KeyNotFoundException)
        {
            code = HttpStatusCode.NotFound;
            message = exception.Message;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        object responseObj;
        if (extraData != null)
        {
            responseObj = new { error = message, existingExchangeId = ((dynamic)extraData).existingExchangeId };
        }
        else
        {
            responseObj = new { error = message };
        }

        var result = JsonSerializer.Serialize(responseObj);
        return context.Response.WriteAsync(result);
    }
}
