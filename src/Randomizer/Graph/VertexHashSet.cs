namespace Randomizer.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Set of vertices, stored as a bitmask indexed by <see cref="Vertex.Id"/>. Set operations are
/// word-wise bit operations over a ulong array; Clone is a flat array copy. This is the central
/// data structure of the search, cloned and intersected heavily by the key-door search.
/// </summary>
public class VertexHashSet : ICollection<Vertex>
{
    // Cached population count; -1 after a bulk set operation invalidates it.
    private int _count;
    private readonly ulong[] _words;

    public Graph Graph { get; }

    public int Count
    {
        get
        {
            if (_count >= 0) return _count;

            int count = BitOps.PopCount(_words);
            _count = count;

            return count;
        }
    }

    public bool IsReadOnly => false;

    public VertexHashSet(Graph graph)
    {
        Graph = graph;
        // Sized to the graph's vertex id space (+1 mirrors the historical sizing; it also keeps
        // at least one word for graphs whose ids are not assigned yet).
        _words = new ulong[(Graph.VertexCount + 64) >> 6];
        _count = 0;
    }

    public VertexHashSet(VertexHashSet other)
    {
        Graph = other.Graph;
        // Measurably faster than array.Clone() (which goes through MemberwiseClone), and this
        // copy is the single hottest allocation of the key-door search.
        _words = GC.AllocateUninitializedArray<ulong>(other._words.Length);
        Array.Copy(other._words, _words, _words.Length);
        _count = other._count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Vertex item)
    {
        Add(item.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int vertexId)
    {
        ref ulong word = ref _words[vertexId >> 6];
        ulong bit = 1UL << vertexId;
        if ((word & bit) == 0)
        {
            word |= bit;
            if (_count != -1)
                _count++;
        }
    }

    public void Clear()
    {
        Array.Clear(_words);
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Vertex item)
    {
        return Contains(item.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int vertexId)
    {
        return (_words[vertexId >> 6] & (1UL << vertexId)) != 0;
    }

    public void CopyTo(Vertex[] array, int arrayIndex)
    {
        var enumerator = GetEnumerator();
        while (enumerator.MoveNext() && arrayIndex < array.Length)
        {
            array[arrayIndex++] = enumerator.Current;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IntersectWith(VertexHashSet other)
    {
        if (other._count == 0)
        {
            Clear();
            return;
        }

        BitOps.And(_words, other._words);
        _count = -1;
    }

    public bool Remove(Vertex item)
    {
        ref ulong word = ref _words[item.Id >> 6];
        ulong bit = 1UL << item.Id;
        bool previous = (word & bit) != 0;
        if (previous)
        {
            word &= ~bit;
            if (_count != -1)
                _count--;
        }

        return previous;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnionWith(VertexHashSet other)
    {
        if (other._count == 0) return;

        BitOps.Or(_words, other._words);
        _count = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExceptWith(VertexHashSet other)
    {
        if (other._count == 0) return;

        BitOps.AndNot(_words, other._words);
        _count = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SymmetricExceptWith(VertexHashSet other)
    {
        if (other._count == 0) return;

        BitOps.Xor(_words, other._words);
        _count = -1;
    }

    public VertexHashSet Clone()
    {
        return new VertexHashSet(this);
    }

    /// <summary>Overwrite this set's contents with another set's, without allocating.</summary>
    public void CopyFrom(VertexHashSet other)
    {
        Array.Copy(other._words, _words, _words.Length);
        _count = other._count;
    }

    /// <summary>Overwrite this set with <paramref name="from"/> minus <paramref name="except"/>, in one pass without allocating.</summary>
    public void CopyFromExcept(VertexHashSet from, VertexHashSet except)
    {
        BitOps.AndNotInto(_words, from._words, except._words);
        _count = -1;
    }

    /// <summary>Build a new set containing <paramref name="from"/> minus <paramref name="except"/>, in one pass (fused Clone + ExceptWith).</summary>
    public static VertexHashSet AndNot(VertexHashSet from, VertexHashSet except)
    {
        var result = new VertexHashSet(from.Graph);
        BitOps.AndNotInto(result._words, from._words, except._words);
        result._count = -1;
        return result;
    }

    public IEnumerator<Vertex> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    private struct Enumerator(VertexHashSet vertices) : IEnumerator, IEnumerator<Vertex>
    {
        private int _wordIndex = -1;
        private ulong _remaining = 0;
        private int _position = -1;

        public readonly Vertex Current => vertices.Graph.GetVertex(_position);

        readonly object IEnumerator.Current => Current;

        public readonly void Dispose()
        {
        }

        public bool MoveNext()
        {
            ulong remaining = _remaining;
            var words = vertices._words;
            int wordIndex = _wordIndex;
            while (remaining == 0)
            {
                wordIndex++;
                if (wordIndex >= words.Length)
                {
                    _wordIndex = wordIndex;
                    _remaining = 0;
                    return false;
                }
                remaining = words[wordIndex];
            }

            _position = (wordIndex << 6) + BitOperations.TrailingZeroCount(remaining);
            _remaining = remaining & (remaining - 1);
            _wordIndex = wordIndex;
            return true;
        }

        public void Reset()
        {
            _wordIndex = -1;
            _remaining = 0;
            _position = -1;
        }
    }
}
