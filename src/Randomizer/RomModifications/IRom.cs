namespace Randomizer.RomModifications;

public interface IRom : IDisposable
{
    void ApplyBasePatch(FileInfo baseBPS);

    /// <summary>resize ROM to a given size</summary>
    /// <param name="size">number of bytes the ROM should be</param>
    void Resize(int size);

    /// <summary>Update the ROM's checksum to be proper</summary>
    void UpdateChecksum();

    /// <summary>
    /// Reads the data at <paramref name="address"/> and returns it.
    /// </summary>
    /// <param name="address">ROM address, defaults to PC. Use <c>(SNES)address</c> to indicate SNES addressing.</param>
    /// <param name="length">Number of bytes to read.</param>
    byte[] Read(Address address, int length);

    /// <summary>Writes <paramref name="data"/> to <paramref name="address"/>.</summary>
    /// <param name="address">ROM address, defaults to PC. Use <c>(SNES)address</c> to indicate SNES addressing.</param>
    /// <param name="data">Data to write.</param>
    void Write(Address address, in ReadOnlySpan<byte> data);

    void Write(Address address, byte value);


}

public readonly struct Address
{
    public int Value { get; init; }
    public static implicit operator Address(int value)
    {
        return new() { Value = value };
    }
}
public readonly struct SNES
{
    public int Value { get; init; }
    public static explicit operator SNES(int value)
    {
        return new() { Value = value };
    }

    public static implicit operator Address(SNES value)
    {
        return new() { Value = ToPC(value.Value) };
    }

    public static SNES operator +(SNES self, int other)
    {
        return new() { Value = self.Value + other };
    }

    public static int ToPC(int address)
    {
        return (address & 0x7F0000) >> 1 | address & 0x7FFF;
    }

    public static int FromPC(int address, bool fastRom = true)
    {
        return ((address << 1) & 0x7F0000) | (address & 0x7FFF) | (fastRom ? 0x808000 : 0x8000);
    }
}
