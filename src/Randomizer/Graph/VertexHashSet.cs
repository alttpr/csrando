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
            if (_bitArray == null) return 0;

            uint[] ints = new uint[(_bitArray.Count >> 5) + 1];
            _bitArray.CopyTo(ints, 0);
            int count = 0;
            for (int i = 0; i < ints.Length; i++)
            {
                count += BitOperations.PopCount(ints[i]);
            }

            _count = count;

            return count;
        }
    }

    public bool IsReadOnly => false;

    public Graph Graph { get; }
    private BitArray? _bitArray;

    public VertexHashSet(Graph graph)
    {
        Graph = graph;
        _count = 0;
    }

    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_bitArray))]
    private void AllocateBitArray()
    {
        int maxId = Graph.GetVertices().Count();
        _bitArray = new BitArray(maxId + 1);
    }

    public VertexHashSet(VertexHashSet other)
    {
        Graph = other.Graph;
        _bitArray = (BitArray?)other._bitArray?.Clone();
        _count = other._count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Vertex item)
    {
        if (_bitArray == null) AllocateBitArray();

        bool previous = _bitArray[item.Id];
        _bitArray.Set(item.Id, true);
        if (!previous && _count != -1)
            _count++;
    }

    public void Clear()
    {
        _bitArray?.SetAll(false);
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Vertex item)
    {
        return _bitArray != null && _bitArray[item.Id];
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
        if (_bitArray == null || other._bitArray == null)
        {
            Clear();
            return;
        }

        _bitArray.And(other._bitArray);
        _count = -1;
    }

    public bool Remove(Vertex item)
    {
        if (_bitArray == null) return false;

        bool previous = _bitArray[item.Id];
        _bitArray.Set(item.Id, false);
        if (previous && _count != -1)
            _count--;

        return previous;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnionWith(VertexHashSet other)
    {
        if (other._bitArray == null) return;

        if (_bitArray == null) AllocateBitArray();

        _bitArray.Or(other._bitArray);
        _count = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExceptWith(VertexHashSet other)
    {
        if (other._bitArray == null) return;

        if (_bitArray == null) AllocateBitArray();

        var otherBitCopy = new BitArray(other._bitArray);
        otherBitCopy.Not();
        _bitArray.And(otherBitCopy);
        _count = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SymmetricExceptWith(VertexHashSet other)
    {
        if (other._bitArray == null) return;

        if (_bitArray == null) AllocateBitArray();

        _bitArray.Xor(other._bitArray);
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    private struct Enumerator(VertexHashSet vertices) : IEnumerator, IEnumerator<Vertex>
    {
        private int _position = -1;

        public readonly Vertex Current => vertices.Graph.GetVertex(_position);

        readonly object IEnumerator.Current => Current;

        public readonly void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (vertices._bitArray is not { } bitArray)
                return false;

            for (; ; )
            {
                _position++;
                if (_position == bitArray.Length) break;
                if (bitArray[_position]) break;
            }
            return _position < bitArray.Length;
        }

        public void Reset()
        {
            _position = -1;
        }
    }
}
