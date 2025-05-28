namespace Task1;

public static class FirstTask
{
    public static IEnumerable<IEnumerable<T>> Zip<T>(
        this IEnumerable<T> first,
        params IEnumerable<T>[] others)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(others);

        return ZipIterator(first, others);
    }

    public static IAsyncEnumerable<IEnumerable<T>> ZipAsync<T>(
        this IAsyncEnumerable<T> first,
        params IAsyncEnumerable<T>[] others)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(others);

        return ZipAsyncIterator(first, others);
    }

    private static IEnumerable<IEnumerable<T>> ZipIterator<T>(
        this IEnumerable<T> first,
        params IEnumerable<T>[] others)
    {
        IEnumerable<IEnumerator<T>> enumerators = others.Select(e => e.GetEnumerator()).ToList();
        using IEnumerator<T> e1 = first.GetEnumerator();
        try
        {
            while (e1.MoveNext() && enumerators.All(e => e.MoveNext()))
            {
                IEnumerable<T> ans = [e1.Current];
                yield return ans.Concat(enumerators.Select(e => e.Current));
            }
        }
        finally
        {
            foreach (IEnumerator<T> enumerator in enumerators)
            {
                enumerator.Dispose();
            }
        }
    }

    private static async IAsyncEnumerable<IEnumerable<T>> ZipAsyncIterator<T>(
        this IAsyncEnumerable<T> first,
        params IAsyncEnumerable<T>[] others)
    {
        var enumerators = others.Select(e => e.GetAsyncEnumerator()).ToList();
        await using IAsyncEnumerator<T> e1 = first.GetAsyncEnumerator();
        try
        {
            while (await e1.MoveNextAsync())
            {
                bool all = true;
                foreach (IAsyncEnumerator<T> e in enumerators)
                {
                    if (await e.MoveNextAsync()) continue;
                    all = false;
                    break;
                }

                if (!all) break;
                IEnumerable<T> ans = [e1.Current];
                yield return ans.Concat(enumerators.Select(e => e.Current));
            }
        }
        finally
        {
            foreach (IAsyncEnumerator<T> enumerator in enumerators)
            {
                await enumerator.DisposeAsync();
            }
        }
    }
}