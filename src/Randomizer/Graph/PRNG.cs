namespace Randomizer.Graph;

using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Random;

public class PRNG
{
    private readonly RandomSource _random;
    public int Seed { get; private set; }

    public PRNG(int? seed)
    {
        Seed = seed ?? RandomSeed.Robust();
        _random = new MersenneTwister(Seed);
    }

    public IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
    {
        return source.OrderBy(_ => _random.Next());
    }

    public void ShuffleSpan<T>(scoped Span<T> source)
    {
        // TODO these numbers can be tuned
        int iterations = source.Length switch
        {
            < 1000 => source.Length * 10,
            < 10000 => source.Length * 3,
            _ => source.Length,
        };

        while (iterations-- > 0)
        {
            int a = _random.Next(source.Length);
            int b = _random.Next(source.Length);

            (source[a], source[b]) = (source[b], source[a]);
        }
    }

    public int GetRandomInt(Range range) => GetRandomInt(range.Start.Value, range.End.Value + 1);
    public int GetRandomInt(int maxExclusive)
    {
        return GetRandomInt(minInclusive: 0, maxExclusive);
    }

    public int GetRandomInt(int minInclusive, int maxExclusive)
    {
        return _random.Next(minInclusive, maxExclusive);
    }

    public T[] Shuffle<T>(T[] array)
    {
        var newArray = new T[array.Length];
        Array.Copy(array, newArray, array.Length);
        int count = array.Length;

        for (int i = count - 1; i > 0; --i)
        {
            int r = GetRandomInt(0, i);
            (newArray[i], newArray[r]) = (newArray[r], newArray[i]);
        }

        return newArray;
    }
    public T GetRandomElement<T>(T[] array)
    {
        return array[GetRandomInt(array.Length)];
    }

    public T GetRandomElement<T>(IEnumerable<T> enumerable)
    {
        if (!enumerable.TryGetNonEnumeratedCount(out int count))
            count = enumerable.Count();
        return enumerable.ElementAt(GetRandomInt(count));
    }

    public IEnumerable<T> GetRandomElements<T>(IEnumerable<T> source, int count) => Shuffle(source).Take(count);
}
