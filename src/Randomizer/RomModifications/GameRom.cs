namespace Randomizer.RomModifications;

public abstract class GameRom(IRom rom, int offset) {
    /// <summary>Writes <paramref name="data"/> to <paramref name="address"/>.</summary>
    /// <param name="address">ROM address, defaults to PC. Use <c>(SNES)address</c> to indicate SNES addressing.</param>
    /// <param name="data">Data to write.</param>
    protected void Write(Address address, in ReadOnlySpan<byte> data) => rom.Write(address.Value + offset, data);

    /// <summary>
    /// Reads the data at <paramref name="address"/> and returns it.
    /// </summary>
    /// <param name="address">ROM address, defaults to PC. Use <c>(SNES)address</c> to indicate SNES addressing.</param>
    /// <param name="length">Number of bytes to read.</param>
    protected byte[] Read(Address address, int length) => rom.Read(address.Value + offset, length);



    protected void Write(Address address, byte value) => rom.Write(address.Value + offset, value);


    protected void WriteUInt16(Address address, scoped ReadOnlySpan<ushort> data) => rom.WriteUInt16(address.Value + offset, data);
}
