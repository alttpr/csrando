namespace Randomizer.RomModifications;

using System.Buffers.Binary;
using BpsNet;

public sealed class Rom : IDisposable
{
    private readonly string _tempRom;
    private readonly FileStream _rom;

    public Rom(string baseRomPath)
    {
        if (!File.Exists(baseRomPath))
            throw new FileNotFoundException("Could not load base ROM file.", baseRomPath);

        _tempRom = Path.GetTempFileName();
        File.Copy(baseRomPath, _tempRom, overwrite: true);
        _rom = new FileStream(_tempRom, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, bufferSize: 32 * 1024, FileOptions.RandomAccess | FileOptions.DeleteOnClose);
    }

    /// <summary>resize ROM to a given size</summary>
    /// <param name="size">number of bytes the ROM should be</param>
    public void Resize(int size) => _rom.SetLength(size);

    public void ApplyBasePatch(FileInfo baseBPS)
    {
        var patcher = new BpsPatch(File.ReadAllBytes(baseBPS.FullName));
        _rom.Seek(0, SeekOrigin.Begin);
        Span<byte> oldRom = new byte[_rom.Length];
        _rom.ReadExactly(oldRom);
        Span<byte> newRom = patcher.Apply(oldRom.ToArray());
        _rom.Seek(0, SeekOrigin.Begin);
        _rom.Write(newRom);
    }

    /// <summary>Update the ROM's checksum to be proper</summary>
    // TODO: this checksum isn't what emulators expect, but fortunately they ignore it.
    public void UpdateChecksum()
    {
        _rom.Seek(0, SeekOrigin.Begin);

        int sum = 0x1FE;
        Span<byte> block = stackalloc byte[1024];
        for (int i = 0; i < _rom.Length; i += block.Length)
        {
            int bytesRead = _rom.Read(block);
            if (bytesRead == 0)
                throw new Exception("Could not read block.");

            for (int j = 0; j < bytesRead; ++j)
            {
                // this skips checksum/inverse in LoROM; HiROM has those at 0xFFDC - 0xFFDF
                // during calculation, they assume 0x0000 and 0xFFFF (which is the initial 0x1FE sum)
                if (j + i >= 0x7FDC && j + i < 0x7FE0)
                    continue;
                sum += block[j];
            }
        }

        ushort checksum = (ushort)(sum & 0xFFFF);
        ushort inverse = (ushort)(checksum ^ 0xFFFF);

        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteUInt16LittleEndian(data, inverse);
        BinaryPrimitives.WriteUInt16LittleEndian(data[2..], checksum);
        Write(0x7FDC, data);
    }

    /// <summary>Save the changes to this output file</summary>
    /// <param name="outputLocation">location on the filesystem to write the new ROM.</param>
    public bool Save(string outputLocation)
    {
        try
        {
            _rom.Flush();
            File.Copy(_tempRom, outputLocation, overwrite: true);
            return true;
        }
        catch { return false; }
    }

    /// <summary>Writes <paramref name="data"/> to <paramref name="address"/>.</summary>
    /// <param name="address">ROM address, defaults to PC. Use <c>(SNES)address</c> to indicate SNES addressing.</param>
    /// <param name="data">Data to write.</param>
    internal void Write(Address address, in ReadOnlySpan<byte> data)
    {
        _rom.Seek(address.Value, SeekOrigin.Begin);
        _rom.Write(data);
    }

    /// <summary>
    /// Reads the data at <paramref name="address"/> and returns it.
    /// </summary>
    /// <param name="address">ROM address, defaults to PC. Use <c>(SNES)address</c> to indicate SNES addressing.</param>
    /// <param name="length">Number of bytes to read.</param>
    internal byte[] Read(Address address, int length)
    {
        _rom.Seek(address.Value, SeekOrigin.Begin);
        var data = new byte[length];
        _rom.ReadExactly(data);
        return data;
    }

    public void Dispose()
    {
        _rom.Dispose();
    }
}

public readonly struct Address
{
    public int Value { get; init; }
    public static implicit operator Address(int value) => new() { Value = value };
}
public readonly struct SNES
{
    public int Value { get; init; }
    public static explicit operator SNES(int value) => new() { Value = value };
    public static implicit operator Address(SNES value) => new() { Value = ToPC(value.Value) };

    public static SNES operator +(SNES self, int other) => new() { Value = self.Value + other };

    public static int ToPC(int address) => (address & 0x7F0000) >> 1 | address & 0x7FFF;
    public static int FromPC(int address, bool fastRom = true) => ((address << 1) & 0x7F0000) | (address & 0x7FFF) | (fastRom ? 0x808000 : 0x8000);
}


