using FluentAssertions;
using NSubstitute;
using Task2.Library;

namespace Task2.Tests;

public class TestsTask2 : IDisposable
{
    private readonly RequestClient _client;
    private readonly CancellationTokenSource _source;
    private readonly CancellationToken _token;
    private readonly ILibraryOperationService _libraryOperationService = Substitute.For<ILibraryOperationService>();

    public TestsTask2()
    {
        _client = new RequestClient(_libraryOperationService);
        _source = new CancellationTokenSource();
        _token = _source.Token;
    }

    [Fact]
    public async Task Test1()
    {
        Guid requestId = Guid.Empty;
        _libraryOperationService.When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(x => requestId = x.Arg<Guid>());
        var requestModel = new RequestModel("http://localhost:5000", [1, 2, 3]);
        var responseModel = new ResponseModel([1, 2]);
        Task<ResponseModel> model = _client.SendAsync(requestModel, _token);
        _client.HandleOperationResult(requestId, requestModel.Data);

        ResponseModel result = await model;
        Assert.Equal(result.Data[0], responseModel.Data[0]);
    }

    [Fact]
    public async Task Test2()
    {
        Guid requestId = Guid.Empty;
        _libraryOperationService.When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(x => requestId = x.Arg<Guid>());
        var requestModel = new RequestModel("http://localhost:5000", [1, 2, 3]);
        Exception ex = new ApplicationException("Test");
        Task<ResponseModel> model = _client.SendAsync(requestModel, _token);
        _client.HandleOperationError(requestId, ex);

        Exception ex2 = await Assert.ThrowsAsync<ApplicationException>(async () => await model);
        Assert.Equal(ex2.Message, ex.Message);
    }

    [Fact]
    public async Task Test3()
    {
        await _source.CancelAsync();
        var requestModel = new RequestModel("http://localhost:5000", [1, 2, 3]);
        Task<ResponseModel> model = _client.SendAsync(requestModel, _token);

        await Assert.ThrowsAsync<TaskCanceledException>(() => model);
    }

    [Fact]
    public async Task Test4()
    {
        var requestModel = new RequestModel("http://localhost:5000", [1, 2, 3]);

        Task<ResponseModel> model = _client.SendAsync(requestModel, _token);

        _source.CancelAfter(100);
        Func<Task> act = async () => await model;
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task Test5()
    {
        var requestModel = new RequestModel("http://localhost:5000", [1, 2, 3]);
        var responseModel = new ResponseModel([1, 2]);
        _libraryOperationService.When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(x => _client.HandleOperationResult(x.Arg<Guid>(), responseModel.Data));

        ResponseModel result = await _client.SendAsync(requestModel, _token);
        Assert.Equal(result.Data[0], responseModel.Data[0]);
    }

    [Fact]
    public async Task Test6()
    {
        var requestModel = new RequestModel("http://localhost:5000", [1, 2, 3]);
        Exception ex = new ApplicationException("Test");
        _libraryOperationService.When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(x => _client.HandleOperationError(x.Arg<Guid>(), ex));

        Task<ResponseModel> model = _client.SendAsync(requestModel, _token);
        Exception ex2 = await Assert.ThrowsAsync<ApplicationException>(async () => await model);
        Assert.Equal(ex2.Message, ex.Message);
    }

    [Fact]
    public async Task Test7()
    {
        var requestModel = new RequestModel("http://localhost:5000", [1, 2, 3]);

        _libraryOperationService.When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(_ => _source.Cancel());

        Task<ResponseModel> model = _client.SendAsync(requestModel, _token);
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await model);
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}