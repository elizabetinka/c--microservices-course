namespace Task2.Library;

public interface ILibraryOperationHandler
{
    void HandleOperationResult(Guid requestId, byte[] data);

    void HandleOperationError(Guid requestId, Exception exception);
}