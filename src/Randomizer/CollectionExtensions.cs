namespace Randomizer;

public static class CollectionExtensions
{
    public static IEnumerable<(int Index, T Value)> Indexed<T>(this IEnumerable<T> collection, Func<T, bool>? predicate = null)
    {
        foreach (var (index, value) in collection.Select((v, i) => (i, v)))
        {
            if (predicate != null && !predicate(value))
                continue;

            yield return (index, value);
        }
    }
}
