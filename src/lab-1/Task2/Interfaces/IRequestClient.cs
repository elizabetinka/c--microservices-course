using Task2.Library;

namespace Task2.Interfaces;

public interface IRequestClient
{
    Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken);
}