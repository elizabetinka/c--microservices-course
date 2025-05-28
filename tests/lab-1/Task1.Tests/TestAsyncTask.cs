using System.Collections;
using Task1.Tests.Data;
using Xunit.Abstractions;

namespace Task1.Tests;

public class TestAsyncTask
{
    private readonly ITestOutputHelper output;

    public TestAsyncTask(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task TestPlayground()
    {
        int[] courses = [2, 4, 7, 8, 10];
        IAsyncEnumerable<int> en2 = AsyncEnumerable.Range(1, courses.Length);
        IAsyncEnumerable<int> en3 = AsyncEnumerable.Range(1, courses.Length);
        IAsyncEnumerable<int> en = AsyncEnumerable.Range(1, courses.Length);
        IAsyncEnumerable<IEnumerable<int>> ans = en.ZipAsync(en2, en3);
        await foreach (IEnumerable<int> enrollment in ans)
        {
            output.WriteLine("new");
            Enumerable.ToList(enrollment).ForEach(x => output.WriteLine(x.ToString()));
        }
    }

    [Fact]
    public async Task WithoutOther()
    {
        IAsyncEnumerable<int> courses = AsyncEnumerable.Range(1, 10);
        IAsyncEnumerable<IEnumerable<int>> result = courses.ZipAsync();

        int[][] ans = [[1], [2], [3], [4], [5], [6], [7], [8], [9], [10]];
        IEnumerator ansEn = ans.GetEnumerator();
        await foreach (IEnumerable<int> enumerable in result)
        {
            ansEn.MoveNext();
            Assert.Equal(enumerable, ansEn.Current);
        }
    }

    [Theory]
    [ClassData(typeof(EqualSizeData))]
    public async Task StandartTest<T>(TestColections<T> colections)
    {
        IAsyncEnumerable<T> first = GetAsyncData(colections.First);
        IEnumerable<IAsyncEnumerable<T>> others = colections.Others.Select(GetAsyncData).ToArray();

        IAsyncEnumerable<IEnumerable<T>> result = first.ZipAsync(others.ToArray());
        using IEnumerator<IEnumerable<T>> ans = colections.Ans.GetEnumerator();
        await foreach (IEnumerable<T> enumerable in result)
        {
            ans.MoveNext();
            Assert.Equal(enumerable, ans.Current);
        }
    }

    [Theory]
    [ClassData(typeof(NotEqualSizeData))]
    public async Task NotEqualSizeTest<T>(TestColections<T> colections)
    {
        IAsyncEnumerable<T> first = GetAsyncData(colections.First);
        IEnumerable<IAsyncEnumerable<T>> others = colections.Others.Select(GetAsyncData).ToArray();

        IAsyncEnumerable<IEnumerable<T>> result = first.ZipAsync(others.ToArray());
        using IEnumerator<IEnumerable<T>> ans = colections.Ans.GetEnumerator();
        await foreach (IEnumerable<T> enumerable in result)
        {
            ans.MoveNext();
            Assert.Equal(enumerable, ans.Current);
        }
    }

    private async IAsyncEnumerable<T> GetAsyncData<T>(IEnumerable<T> enumerable)
    {
        foreach (T item in enumerable)
        {
            await Task.Delay(1);
            yield return item;
        }
    }
}