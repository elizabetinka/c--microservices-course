using System.Collections;

namespace Task1.Tests.Data;

public class EqualSizeData : IEnumerable<object?[]>
{
    private readonly List<object?[]> _data =
    [
        [new TestColections<int>([1, 2, 3, 4, 5, 6], [[2, 3, 4, 5, 6, 7], [3, 4, 5, 6, 7, 8], [4, 5, 6, 7, 8, 9]], [[1, 2, 3, 4], [2, 3, 4, 5], [3, 4, 5, 6], [4, 5, 6, 7], [5, 6, 7, 8], [6, 7, 8, 9]])],
        [new TestColections<string>(["aa", "bb", "cc", "dd", "ff", "gg"], [["bb", "cc", "dd", "ff", "gg", "ee"], ["cc", "dd", "ff", "gg", "ee", "hh"]], [["aa", "bb", "cc"], ["bb", "cc", "dd"], ["cc", "dd", "ff"], ["dd", "ff", "gg"], ["ff", "gg", "ee"], ["gg", "ee", "hh"]])],
        [new TestColections<int>([1, 2], [[2, 3], [3, 4], [4, 5], [5, 6], [6, 7], [7, 8], [8, 9], [9, 10]], [[1, 2, 3, 4, 5, 6, 7, 8, 9], [2, 3, 4, 5, 6, 7, 8, 9, 10]])],
    ];

    public IEnumerator<object?[]> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}