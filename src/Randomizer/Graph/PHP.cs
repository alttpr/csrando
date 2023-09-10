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

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(_ => Random.Shared.Next());
    }

    private static Random _rns = Random.Shared;
    public static void SetRandomizer(Random source) { _rns = source ?? throw new ArgumentNullException(nameof(source)); }
    public static int get_random_int(int max)
    {
        return get_random_int(min: 0, max);
    }

    public static int get_random_int(int min, int max)
    {
        return _rns.Next(min, max);
    }

    public static T[] fy_shuffle<T>(T[] array)
    {
        var new_array = new T[array.Length];
        Array.Copy(array, new_array, array.Length);
        int count = array.Length;

        for (int i = count - 1; i >= 0; --i)
        {
            int r = get_random_int(0, i);
            (new_array[i], new_array[r]) = (new_array[r], new_array[i]);
        }

        return new_array;
    }
    public static T get_random_element<T>(T[] array)
    {
        return array[get_random_int(array.Length)];
    }

    public static T get_random_element<T>(IEnumerable<T> array)
    {
        return array.ElementAt(get_random_int(array.Count()));
    }
}
