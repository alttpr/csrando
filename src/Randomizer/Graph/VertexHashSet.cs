namespace Randomizer.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

public class VertexHashSet : ICollection<Vertex>
{
    private int _count = -1;
    public int Count
    {
        get
        {
            if (_count >= 0) return _count;
            if (_bits == null) return 0;
            int c = 0;
            foreach (var w in _bits)
                c += BitOperations.PopCount(w);
            _count = c;
            return c;
        }
    }

    public bool IsReadOnly => false;

    public Graph Graph { get; }
    private uint[]? _bits;

    public VertexHashSet(Graph graph)
    {
        Graph = graph;
        _count = 0;
    }

    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_bits))]
    private void AllocateBits()
    {
        int maxId = Graph.GetVertices().Count();
        int words = ((maxId + 1) + 31) >> 5;
        _bits = new uint[Math.Max(1, words)];
    }

    public VertexHashSet(VertexHashSet other)
    {
        Graph = other.Graph;
        _bits = other._bits is null ? null : (uint[])other._bits.Clone();
        _count = other._count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Vertex item)
    {
        if (_bits == null) AllocateBits();
        int id = item.Id;
        int idx = id >> 5;
        uint mask = 1u << (id & 31);
        bool previous = (_bits[idx] & mask) != 0;
        _bits[idx] |= mask;
        if (!previous && _count != -1) _count++;
    }

    public void Clear()
    {
        if (_bits != null)
            Array.Clear(_bits, 0, _bits.Length);
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Vertex item)
    {
        return _bits != null && ((_bits[item.Id >> 5] & (1u << (item.Id & 31))) != 0);
    }

    public void CopyTo(Vertex[] array, int arrayIndex)
    {
        foreach (var v in this)
        {
            if (arrayIndex >= array.Length) break;
            array[arrayIndex++] = v;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IntersectWith(VertexHashSet other)
    {
        if (_bits == null || other._bits == null)
        {
            Clear();
            return;
        }
        int n = Math.Min(_bits.Length, other._bits.Length);
        for (int i = 0; i < n; i++) _bits[i] &= other._bits[i];
        for (int i = n; i < _bits.Length; i++) _bits[i] = 0;
        _count = -1;
    }

    public bool Remove(Vertex item)
    {
        if (_bits == null) return false;
        int id = item.Id;
        int idx = id >> 5;
        uint mask = 1u << (id & 31);
        bool previous = (_bits[idx] & mask) != 0;
        _bits[idx] &= ~mask;
        if (previous && _count != -1) _count--;
        return previous;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnionWith(VertexHashSet other)
    {
        if (other._bits == null) return;
        if (_bits == null) AllocateBits();
        int n = Math.Min(_bits.Length, other._bits.Length);
        for (int i = 0; i < n; i++) _bits[i] |= other._bits[i];
        _count = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExceptWith(VertexHashSet other)
    {
        if (other._bits == null) return;
        if (_bits == null) AllocateBits();
        int n = Math.Min(_bits.Length, other._bits.Length);
        for (int i = 0; i < n; i++) _bits[i] &= ~other._bits[i];
        _count = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SymmetricExceptWith(VertexHashSet other)
    {
        if (other._bits == null) return;
        if (_bits == null) AllocateBits();
        int n = Math.Min(_bits.Length, other._bits.Length);
        for (int i = 0; i < n; i++) _bits[i] ^= other._bits[i];
        _count = -1;
    }

    public VertexHashSet Clone()
    {
        return new VertexHashSet(this);
    }

    public IEnumerator<Vertex> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private struct Enumerator : IEnumerator<Vertex>
    {
        private readonly VertexHashSet _set;
        private int _wordIndex;
        private uint _word;
        private int _baseId;
        private Vertex _current = null!;

        public Enumerator(VertexHashSet set)
        {
            _set = set;
            _wordIndex = -1;
            _word = 0;
            _baseId = 0;
        }

        public Vertex Current => _current;
        object IEnumerator.Current => Current;
        public void Dispose() { }

        public bool MoveNext()
        {
            var bits = _set._bits;
            if (bits == null) return false;
            while (true)
            {
                if (_word != 0)
                {
                    int tz = BitOperations.TrailingZeroCount(_word);
                    int id = _baseId + tz;
                    _word &= _word - 1; // clear lowest set bit
                    _current = _set.Graph.GetVertex(id);
                    return true;
                }
                _wordIndex++;
                if (_wordIndex >= bits.Length) return false;
                _word = bits[_wordIndex];
                _baseId = _wordIndex << 5;
            }
        }

        public void Reset()
        {
            _wordIndex = -1;
            _word = 0;
            _baseId = 0;
        }
    }

    // Classic enumerator removed; bitset enumerator is the default.
}
