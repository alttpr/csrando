namespace AlttpRandomizer.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public class PRNG
{
    private readonly Random _random = new Random();
    public Int32 Seed { get; private set; }

    public PRNG(Int32? seed)
    {
        Seed = seed ?? RandomNumberGenerator.GetInt32(Int32.MaxValue);
        _random = new Random(Seed);
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
        var new_array = new T[array.Length];
        Array.Copy(array, new_array, array.Length);
        int count = array.Length;

        for (int i = count - 1; i >= 0; --i)
        {
            int r = GetRandomInt(0, i);
            (new_array[i], new_array[r]) = (new_array[r], new_array[i]);
        }

        return new_array;
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
