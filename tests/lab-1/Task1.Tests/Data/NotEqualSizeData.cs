using System.Collections;

namespace Task1.Tests.Data;

public class NotEqualSizeData : IEnumerable<object?[]>
{
    private readonly List<object?[]> _data =
    [
        [new TestColections<int>(
            [1, 2, 3, 4, 5, 6],
            [[2, 3, 4], [3, 4, 5, 6, 7, 8, 9], [4, 5, 6, 7]],
            [[1, 2, 3, 4], [2, 3, 4, 5], [3, 4, 5, 6]])],
        [
            new TestColections<string>(
                ["aa", "bb", "cc"],
                [["bb", "cc", "dd", "ff", "gg", "ee"], ["cc", "dd", "ff", "gg"]],
                [["aa", "bb", "cc"], ["bb", "cc", "dd"], ["cc", "dd", "ff"]])
        ],
        [
            new TestColections<int>(
                [1, 2],
                [[2, 3], [3, 4, 10], [4, 5], [5], [6, 7, 8], [7, 8], [8, 9], [9, 10, 9]],
                [[1, 2, 3, 4, 5, 6, 7, 8, 9]]),
        ],
        [
            new TestColections<int>([1, 2], [[], [3, 4, 10], [4, 5], [6], [6, 7, 8], [7, 8], [8, 9], [9, 10, 9]], [])
        ]
    ];

    public IEnumerator<object?[]> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}