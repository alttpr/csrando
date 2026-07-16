namespace Randomizer.Graph;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

/// <summary>
/// Vectorized bulk operations over ulong bitmask arrays, used by <see cref="VertexHashSet"/>.
/// Both arrays must be the same length (sets over the same graph always are).
///
/// The main loops use 512-bit vectors when the hardware supports them, otherwise the platform
/// vector width. For the idempotent operations (or / and / and-not) the trailing partial block
/// is handled by one full-width vector that overlaps the previous block — re-applying an
/// idempotent operation to the overlap is harmless and cheaper than a scalar tail. XOR is not
/// idempotent, so it keeps a scalar tail.
/// </summary>
internal static class BitOps
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Or(ulong[] a, ulong[] b)
    {
        ref ulong ra = ref MemoryMarshal.GetArrayDataReference(a);
        ref ulong rb = ref MemoryMarshal.GetArrayDataReference(b);
        int length = a.Length;

        if (Vector512.IsHardwareAccelerated && length >= Vector512<ulong>.Count)
        {
            int i = 0;
            int last = length - Vector512<ulong>.Count;
            for (; i < last; i += Vector512<ulong>.Count)
            {
                Vector512.StoreUnsafe(Vector512.LoadUnsafe(ref ra, (nuint)i) | Vector512.LoadUnsafe(ref rb, (nuint)i), ref ra, (nuint)i);
            }
            Vector512.StoreUnsafe(Vector512.LoadUnsafe(ref ra, (nuint)last) | Vector512.LoadUnsafe(ref rb, (nuint)last), ref ra, (nuint)last);
        }
        else if (Vector.IsHardwareAccelerated && length >= Vector<ulong>.Count)
        {
            int i = 0;
            int last = length - Vector<ulong>.Count;
            for (; i < last; i += Vector<ulong>.Count)
            {
                Vector.StoreUnsafe(Vector.LoadUnsafe(ref ra, (nuint)i) | Vector.LoadUnsafe(ref rb, (nuint)i), ref ra, (nuint)i);
            }
            Vector.StoreUnsafe(Vector.LoadUnsafe(ref ra, (nuint)last) | Vector.LoadUnsafe(ref rb, (nuint)last), ref ra, (nuint)last);
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                a[i] |= b[i];
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void And(ulong[] a, ulong[] b)
    {
        ref ulong ra = ref MemoryMarshal.GetArrayDataReference(a);
        ref ulong rb = ref MemoryMarshal.GetArrayDataReference(b);
        int length = a.Length;

        if (Vector512.IsHardwareAccelerated && length >= Vector512<ulong>.Count)
        {
            int i = 0;
            int last = length - Vector512<ulong>.Count;
            for (; i < last; i += Vector512<ulong>.Count)
            {
                Vector512.StoreUnsafe(Vector512.LoadUnsafe(ref ra, (nuint)i) & Vector512.LoadUnsafe(ref rb, (nuint)i), ref ra, (nuint)i);
            }
            Vector512.StoreUnsafe(Vector512.LoadUnsafe(ref ra, (nuint)last) & Vector512.LoadUnsafe(ref rb, (nuint)last), ref ra, (nuint)last);
        }
        else if (Vector.IsHardwareAccelerated && length >= Vector<ulong>.Count)
        {
            int i = 0;
            int last = length - Vector<ulong>.Count;
            for (; i < last; i += Vector<ulong>.Count)
            {
                Vector.StoreUnsafe(Vector.LoadUnsafe(ref ra, (nuint)i) & Vector.LoadUnsafe(ref rb, (nuint)i), ref ra, (nuint)i);
            }
            Vector.StoreUnsafe(Vector.LoadUnsafe(ref ra, (nuint)last) & Vector.LoadUnsafe(ref rb, (nuint)last), ref ra, (nuint)last);
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                a[i] &= b[i];
            }
        }
    }

    /// <summary>a &amp;= ~b</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AndNot(ulong[] a, ulong[] b)
    {
        ref ulong ra = ref MemoryMarshal.GetArrayDataReference(a);
        ref ulong rb = ref MemoryMarshal.GetArrayDataReference(b);
        int length = a.Length;

        if (Vector512.IsHardwareAccelerated && length >= Vector512<ulong>.Count)
        {
            int i = 0;
            int last = length - Vector512<ulong>.Count;
            for (; i < last; i += Vector512<ulong>.Count)
            {
                Vector512.StoreUnsafe(Vector512.AndNot(Vector512.LoadUnsafe(ref ra, (nuint)i), Vector512.LoadUnsafe(ref rb, (nuint)i)), ref ra, (nuint)i);
            }
            Vector512.StoreUnsafe(Vector512.AndNot(Vector512.LoadUnsafe(ref ra, (nuint)last), Vector512.LoadUnsafe(ref rb, (nuint)last)), ref ra, (nuint)last);
        }
        else if (Vector.IsHardwareAccelerated && length >= Vector<ulong>.Count)
        {
            int i = 0;
            int last = length - Vector<ulong>.Count;
            for (; i < last; i += Vector<ulong>.Count)
            {
                Vector.StoreUnsafe(Vector.AndNot(Vector.LoadUnsafe(ref ra, (nuint)i), Vector.LoadUnsafe(ref rb, (nuint)i)), ref ra, (nuint)i);
            }
            Vector.StoreUnsafe(Vector.AndNot(Vector.LoadUnsafe(ref ra, (nuint)last), Vector.LoadUnsafe(ref rb, (nuint)last)), ref ra, (nuint)last);
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                a[i] &= ~b[i];
            }
        }
    }

    /// <summary>dest = a &amp; ~b, in a single pass (dest may not alias a or b).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AndNotInto(ulong[] dest, ulong[] a, ulong[] b)
    {
        ref ulong rd = ref MemoryMarshal.GetArrayDataReference(dest);
        ref ulong ra = ref MemoryMarshal.GetArrayDataReference(a);
        ref ulong rb = ref MemoryMarshal.GetArrayDataReference(b);
        int length = dest.Length;

        if (Vector512.IsHardwareAccelerated && length >= Vector512<ulong>.Count)
        {
            int i = 0;
            int last = length - Vector512<ulong>.Count;
            for (; i < last; i += Vector512<ulong>.Count)
            {
                Vector512.StoreUnsafe(Vector512.AndNot(Vector512.LoadUnsafe(ref ra, (nuint)i), Vector512.LoadUnsafe(ref rb, (nuint)i)), ref rd, (nuint)i);
            }
            Vector512.StoreUnsafe(Vector512.AndNot(Vector512.LoadUnsafe(ref ra, (nuint)last), Vector512.LoadUnsafe(ref rb, (nuint)last)), ref rd, (nuint)last);
        }
        else if (Vector.IsHardwareAccelerated && length >= Vector<ulong>.Count)
        {
            int i = 0;
            int last = length - Vector<ulong>.Count;
            for (; i < last; i += Vector<ulong>.Count)
            {
                Vector.StoreUnsafe(Vector.AndNot(Vector.LoadUnsafe(ref ra, (nuint)i), Vector.LoadUnsafe(ref rb, (nuint)i)), ref rd, (nuint)i);
            }
            Vector.StoreUnsafe(Vector.AndNot(Vector.LoadUnsafe(ref ra, (nuint)last), Vector.LoadUnsafe(ref rb, (nuint)last)), ref rd, (nuint)last);
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                dest[i] = a[i] & ~b[i];
            }
        }
    }

    /// <summary>a ^= b. XOR is not idempotent, so the tail stays scalar.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Xor(ulong[] a, ulong[] b)
    {
        ref ulong ra = ref MemoryMarshal.GetArrayDataReference(a);
        ref ulong rb = ref MemoryMarshal.GetArrayDataReference(b);
        int length = a.Length;
        int i = 0;

        if (Vector512.IsHardwareAccelerated)
        {
            for (; i <= length - Vector512<ulong>.Count; i += Vector512<ulong>.Count)
            {
                Vector512.StoreUnsafe(Vector512.LoadUnsafe(ref ra, (nuint)i) ^ Vector512.LoadUnsafe(ref rb, (nuint)i), ref ra, (nuint)i);
            }
        }
        else if (Vector.IsHardwareAccelerated)
        {
            for (; i <= length - Vector<ulong>.Count; i += Vector<ulong>.Count)
            {
                Vector.StoreUnsafe(Vector.LoadUnsafe(ref ra, (nuint)i) ^ Vector.LoadUnsafe(ref rb, (nuint)i), ref ra, (nuint)i);
            }
        }

        for (; i < length; i++)
        {
            a[i] ^= b[i];
        }
    }

    public static int PopCount(ulong[] a)
    {
        int count = 0;
        foreach (ulong word in a)
        {
            count += BitOperations.PopCount(word);
        }

        return count;
    }
}
