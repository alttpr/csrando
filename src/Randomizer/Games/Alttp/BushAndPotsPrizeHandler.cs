namespace Randomizer.Games.Alttp;

using System.Numerics;
using System.Runtime.Intrinsics.X86;

public sealed class BushAndPotsPrizeHandler
{
    private readonly byte[] _spriteIds = [
        0xD9, // 0x00 - green rupee
        0x3E, // 0x01 - hoarder
        0x79, // 0x02 - bee
        0xD9, // 0x03 - green rupee
        0xDC, // 0x04 - 1 bomb
        0xD8, // 0x05 - heart
        0xDA, // 0x06 - blue rupee
        0xE4, // 0x07 - small key
        0xE1, // 0x08 - 5 arrows
        0xDC, // 0x09 - 1 bomb
        0xD8, // 0x0A - heart
        0xDF, // 0x0B - small magic
        0xE0, // 0x0C - full magic
        0x0B, // 0x0D - cucco
        0x42, // 0x0E - green guard
        0xD3, // 0x0F - stal
        0x41, // 0x10 - blue guard
        0xD4, // 0x11 - landmine
        0xD9, // 0x12 - green rupee
        0xE3, // 0x13 - fairy
        0xD8, // 0x14 - heart
     ];

    private readonly ulong[] _bannedSlots = new ulong[0x64];

    private readonly byte[] _bushPrizeIndices = [
        0x01, 0x01, 0x01, 0x01, 0x0F, 0x01, 0x01, 0x12, // LW
        0x10, 0x01, 0x01, 0x01, 0x11, 0x01, 0x01, 0x03 // DW
        ];

    public Span<byte> BushPrizeIndicesLW => _bushPrizeIndices.AsSpan(0, 8);
    public Span<byte> BushPrizeIndicesDW => _bushPrizeIndices.AsSpan(8, 8);
    public ReadOnlySpan<byte> SpriteIds => _spriteIds;

    public void BanSlotOnScreen(int screen, int slot)
    {
        _bannedSlots[screen] |= (byte) (1 << slot);
    }

    public byte[] GetBanMasks(out byte[] prizeMasks)
    {
        ulong allBannedSlots = 0;

        foreach (ulong l in _bannedSlots)
        {
            allBannedSlots |= l;
        }

        int count = BitOperations.PopCount(allBannedSlots);

        if (count > 8)
        {
            throw new InvalidOperationException("Too many banned pot sprites!");
        }

        Span<byte> bannedSlots = stackalloc byte[8];

        int slot = 0;
        byte index = 0;
        while (allBannedSlots > 0)
        {
            if ((allBannedSlots & 1) is 1)
            {
                bannedSlots[slot++] = index;
            }
            index++;
        }

        prizeMasks = new byte[16];

        for (int banIndex = 0; banIndex < _bushPrizeIndices.Length; banIndex++)
        {
            int maskIndex = bannedSlots.IndexOf(_bushPrizeIndices[banIndex]);

            if (maskIndex > 0)
                prizeMasks[banIndex] = (byte) (1 << maskIndex);
        }

        byte[] screenBanMask = new byte[64];

        //
        // now that every mask has been set up, we need to reconcile the ulong masks with the byte masks
        //
        if (Bmi2.X64.IsSupported)
        {
            // this is insanely trivial with PEXT
            for (int i = 0; i < _bannedSlots.Length; i++)
            {
                screenBanMask[i & 0x3F] |= (byte) Bmi2.X64.ParallelBitExtract(_bannedSlots[i], allBannedSlots);
            }
        }
        else
        {
            throw new NotImplementedException(); // TODO: this will suck to write!
        }

        return screenBanMask;
    }

    // indices off by 1, because table has no 0 entry
    public ref byte Prize01SpriteId => ref _spriteIds[0x00];
    public ref byte Prize02SpriteId => ref _spriteIds[0x01];
    public ref byte Prize03SpriteId => ref _spriteIds[0x02];
    public ref byte Prize04SpriteId => ref _spriteIds[0x03];
    public ref byte Prize05SpriteId => ref _spriteIds[0x04];
    public ref byte Prize06SpriteId => ref _spriteIds[0x05];
    public ref byte Prize07SpriteId => ref _spriteIds[0x06];
    public byte Prize08SpriteId => _spriteIds[0x07]; // read only, because it's the key
    public ref byte Prize09SpriteId => ref _spriteIds[0x08];
    public ref byte Prize0ASpriteId => ref _spriteIds[0x09];
    public ref byte Prize0BSpriteId => ref _spriteIds[0x0A];
    public ref byte Prize0CSpriteId => ref _spriteIds[0x0B];
    public ref byte Prize0DSpriteId => ref _spriteIds[0x0C];
    public ref byte Prize0ESpriteId => ref _spriteIds[0x0D];
    public ref byte Prize0FSpriteId => ref _spriteIds[0x0E];
    public ref byte Prize10SpriteId => ref _spriteIds[0x0F];
    public ref byte Prize11SpriteId => ref _spriteIds[0x10];
    public ref byte Prize12SpriteId => ref _spriteIds[0x11];
    public ref byte Prize13SpriteId => ref _spriteIds[0x12];
    public ref byte Prize14SpriteId => ref _spriteIds[0x13];
    public ref byte Prize15SpriteId => ref _spriteIds[0x14];
}
