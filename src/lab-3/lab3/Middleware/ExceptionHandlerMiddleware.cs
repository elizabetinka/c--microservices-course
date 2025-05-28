using Npgsql;
using System.Net;

namespace Lab3.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext content)
    {
        try
        {
            await _next(content);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(content, e);
        }
    }

    private Task HandleExceptionAsync(HttpContext content, Exception exception)
    {
        HttpStatusCode code = HttpStatusCode.InternalServerError;
        string message = exception.Message;
        switch (exception)
        {
            case NpgsqlOperationInProgressException:
                break;
            case ArgumentException: case NpgsqlException:
                code = HttpStatusCode.BadRequest;
                break;
            default:
                code = HttpStatusCode.InternalServerError;
                message = string.Empty;
                break;
        }

        _logger.LogError(message);
        content.Response.ContentType = "text/plain";
        content.Response.StatusCode = (int)code;
        return content.Response.WriteAsync(message);
    }
}