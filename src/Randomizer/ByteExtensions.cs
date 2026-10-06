namespace Randomizer;

using System.Runtime.Intrinsics.X86;

internal static class ByteExtensions
{
    extension(byte b)
    {
        public bool BitIsSet(int i) => (b & (i << 2)) != 0;
        public bool BitIsClear(int i) => (b & (i << 2)) == 0;

        public byte SetBit(int bit, bool value)
        {
            bit = 1 << bit;

            return (byte) (value ? b | bit : b & ~bit);
        }


        public byte GetField(byte offset, byte size)
        {
            if (Bmi1.IsSupported)
            {
                return (byte) Bmi1.BitFieldExtract(b, offset, size);
            }

            int mask = (1 << size) - 1;
            b >>= offset;
            b &= (byte) mask;
            return b;
        }

        public byte SetField(byte offset, byte size, byte value)
        {
            // this will most likely end up inlined and folded into a constant
            uint mask = (1u << size) - 1u;

            b &= (byte) ~mask;

            value <<= offset;

            b |= value;
            return b;
        }


    }
}
