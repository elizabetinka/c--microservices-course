using Task1.Tests.Data;
using Xunit.Abstractions;

namespace Task1.Tests;

public class TestSyncTask
{
    private readonly ITestOutputHelper output;

    public TestSyncTask(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void TestPlayground()
    {
        int[] courses = [2, 4, 7, 8, 10];
        var students = new List<int> { 2, 1 };
        var students2 = new List<int> { 5, 2 };
        var students3 = new List<int> { 5, 2, 80 };

        IEnumerable<IEnumerable<int>> enrollments = courses.Zip(students, students2, students3);

        foreach (IEnumerable<int> enrollment in enrollments)
        {
            output.WriteLine("new");
            Enumerable.ToList(enrollment).ForEach(x => output.WriteLine(x.ToString()));
        }
    }

    [Fact]
    public void WithoutOther()
    {
        int[] courses = [2, 4, 7, 8, 10];
        int[][] ans = [ [2], [4], [7], [8], [10]];

        Assert.Equal(courses.Zip(), ans);
    }

    [Theory]
    [ClassData(typeof(EqualSizeData))]
    public void StandartTest<T>(TestColections<T> colections)
    {
        Assert.Equal(colections.First.Zip(colections.Others.ToArray()), colections.Ans);
    }

    [Theory]
    [ClassData(typeof(NotEqualSizeData))]
    public void NotEqualSizeTest<T>(TestColections<T> colections)
    {
        Assert.Equal(colections.First.Zip(colections.Others.ToArray()), colections.Ans);
    }
}