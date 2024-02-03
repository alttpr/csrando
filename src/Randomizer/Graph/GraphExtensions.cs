namespace Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class GraphExtensions
{
    public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> source)
    {
        T[] elements = source.ToArray();
        int combinationCount = 1 << elements.Length; // 2^N combinations

        for (int i = 1; i < combinationCount; i++) // Start at 1 to exclude the empty set
        {
            var subset = new List<T>();
            for (int j = 0; j < elements.Length; j++)
            {
                if ((i & (1 << j)) != 0)
                {
                    subset.Add(elements[j]);
                }
            }
            yield return subset;
        }
    }
}
