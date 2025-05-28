namespace Task2.Library;

public interface ILibraryOperationService
{
    void BeginOperation(Guid requestId, RequestModel model, CancellationToken cancellationToken);
}