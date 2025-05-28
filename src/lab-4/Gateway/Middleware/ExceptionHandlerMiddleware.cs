using Grpc.Core;
using System.Net;

namespace Gateway.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext content)
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

    private async Task HandleExceptionAsync(HttpContext content, Exception exception)
    {
        int code = (int)HttpStatusCode.InternalServerError;
        string message = exception.Message;
        switch (exception)
        {
            case RpcException rpcException:
                code = (int)rpcException.StatusCode;
                message = rpcException.Status.Detail.ToString();
                break;
            default:
                code = (int)HttpStatusCode.InternalServerError;
                message = string.Empty;
                break;
        }

        _logger.LogError(message);

        content.Response.ContentType = "text/plain";
        content.Response.StatusCode = code;
        await content.Response.WriteAsync(message);
    }
}