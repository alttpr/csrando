namespace Randomizer.Graph;

using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public void ShuffleInPlace<T>(IList<T> list)
    {
        int count = list.Count;
        for (int i = count - 1; i > 0; --i)
        {
            int r = GetRandomInt(0, i);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
    public T GetRandomElement<T>(T[] array)
    {
        return array[GetRandomInt(array.Length)];
    }

    public T GetRandomElement<T>(IEnumerable<T> array)
    {
        if (array is IList<T> list)
        {
            return list[GetRandomInt(list.Count)];
        }
        // Fallback: single pass with reservoir-like selection
        int idx = 0;
        T? selected = default;
        foreach (var item in array)
        {
            if (GetRandomInt(++idx) == 0)
                selected = item;
        }
        if (idx == 0)
            throw new InvalidOperationException("Sequence was empty");
        return selected!;
    }

    public IEnumerable<T> GetRandomElements<T>(IEnumerable<T> source, int count) => Shuffle(source).Take(count);
}
