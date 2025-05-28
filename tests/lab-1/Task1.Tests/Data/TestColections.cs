namespace Task1.Tests.Data;

public readonly struct TestColections<T>(
    IEnumerable<T> first,
    IEnumerable<IEnumerable<T>> others,
    IEnumerable<IEnumerable<T>> ans)
{
    public IEnumerable<T> First { get; } = first;

    public IEnumerable<IEnumerable<T>> Others { get; } = others;

    public IEnumerable<IEnumerable<T>> Ans { get; } = ans;
}