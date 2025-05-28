using System.Collections.Concurrent;
using Task2.Interfaces;
using Task2.Library;

namespace Task2;

public class RequestClient(ILibraryOperationService libraryOperationService) : IRequestClient, ILibraryOperationHandler
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResponseModel>> _taskCompletionSources = new();

    public async Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return await Task.FromCanceled<ResponseModel>(cancellationToken);
        }

        var reqId = Guid.NewGuid();
        var tcs = new TaskCompletionSource<ResponseModel>();

        _taskCompletionSources[reqId] = tcs;
        await using CancellationTokenRegistration cancellationRegistration = cancellationToken.Register(
           () =>
           {
               if (_taskCompletionSources.TryRemove(reqId, out _))
               {
                   tcs.TrySetCanceled(cancellationToken);
               }
           });

        try
        {
            libraryOperationService.BeginOperation(reqId, request, cancellationToken);
        }
        catch (Exception ex)
        {
            if (_taskCompletionSources.TryRemove(reqId, out _))
            {
                tcs.TrySetException(ex);
            }
        }

        return await tcs.Task;
    }

    public void HandleOperationResult(Guid requestId, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(requestId);
        ArgumentNullException.ThrowIfNull(data);

        if (_taskCompletionSources.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? tcs))
        {
            tcs.TrySetResult(new ResponseModel(data));
        }
    }

    public void HandleOperationError(Guid requestId, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(requestId);
        ArgumentNullException.ThrowIfNull(exception);

        if (_taskCompletionSources.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? tcs))
        {
            tcs.TrySetException(exception);
        }
    }
}