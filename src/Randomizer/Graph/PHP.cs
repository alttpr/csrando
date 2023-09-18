namespace AlttpRandomizer.Graph;

internal static class PHP
{
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> array, params IDictionary<TKey, TValue>[] arrays)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>(array);
        foreach (var (key, value) in arrays.SelectMany(d => d))
            result[key] = value;

        return result;
    }
    public static Dictionary<TKey, TValue> MergeOne<TKey, TValue>(this IDictionary<TKey, TValue> array, TKey key, TValue value)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>(array);
        result[key] = value;
        return result;
    }
    public static Dictionary<TKey, TValue> Diff<TKey, TValue>(this IDictionary<TKey, TValue> array, params TValue[] arrays)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>(array);
        var comparer = EqualityComparer<TValue>.Default;
        foreach (var value in arrays.Select(d => d))
            result.Remove(result.FirstOrDefault(e => comparer.Equals(e.Value, value)).Key);

        return result;
    }
}
