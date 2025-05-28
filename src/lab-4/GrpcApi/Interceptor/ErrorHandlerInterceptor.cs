using Grpc.Core;

namespace GrpcApi.Interceptor;

public class ErrorHandlerInterceptor : Grpc.Core.Interceptors.Interceptor
{
    private readonly ILogger _logger;

    public ErrorHandlerInterceptor(ILogger<ErrorHandlerInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("Starting receiving call. Type/Method: {Type} / {Method}", MethodType.Unary, context.Method);
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error thrown by {context.Method}.");
            throw;
        }
    }
}