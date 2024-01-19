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

    public int GetRandomInt(int max)
    {
        return GetRandomInt(min: 0, max);
    }

    public int GetRandomInt(int min, int max)
    {
        return _random.Next(min, max);
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

    public T GetRandomElement<T>(IEnumerable<T> array)
    {
        return array.ElementAt(GetRandomInt(array.Count()));
    }
}
