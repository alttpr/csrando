namespace Randomizer.RomModifications;

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using BpsNet;

public sealed class FileRom : IRom
{
    private readonly string _tempRom;
    private readonly FileStream _rom;

    public FileRom(string baseRomPath)
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
    // TODO add a flag so that front ends can skip checksum when they calculate their own
    // TODO use Futaba.Snes.RomHeader.CalculateChecksum once we bring that in for building
    public void UpdateChecksum()
    {
        int loc = IRom.CheckSumLocationLorom;

        // dummy checksum to satisfy summing
        Write(loc, [0xFF, 0xFF, 0x00, 0x00]);

        _rom.Seek(0, SeekOrigin.Begin);

        uint sum = 0;

        Span<byte> block = stackalloc byte[1024];
        for (int i = 0; i < _rom.Length; i += block.Length)
        {
            int bytesRead = _rom.Read(block);

            if (bytesRead == 0) throw new Exception("Could not read block.");

            foreach (byte b in block[..bytesRead])
            {
                sum += b;
            }
        }

        ushort checksum = (ushort) sum;
        ushort inverse = (ushort)(checksum ^ 0xFFFF);

        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteUInt16LittleEndian(data, inverse);
        BinaryPrimitives.WriteUInt16LittleEndian(data[2..], checksum);
        Write(loc, data);
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
    public void Write(Address address, in ReadOnlySpan<byte> data)
    {
        _rom.Seek(address.Value, SeekOrigin.Begin);
        _rom.Write(data);
    }

    /// <summary>
    /// Reads the data at <paramref name="address"/> and returns it.
    /// </summary>
    /// <param name="address">ROM address, defaults to PC. Use <c>(SNES)address</c> to indicate SNES addressing.</param>
    /// <param name="length">Number of bytes to read.</param>
    public byte[] Read(Address address, int length)
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


    public void Write(Address address, byte value)
    {
        Write(address, [value]);
    }

    public void WriteUInt16(Address address, in ReadOnlySpan<ushort> data)
    {
        if (BitConverter.IsLittleEndian)
        {
            Write(address, MemoryMarshal.AsBytes(data));
        }
        else
        {
            int len = data.Length * sizeof(ushort);
            byte[] tmp = ArrayPool<byte>.Shared.Rent(len);

            try
            {
                Span<byte> tmpSpan = tmp.AsSpan(0, len);

                var tmpSpanWrite = tmpSpan;
                foreach (ushort value in data)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(tmpSpanWrite, value);
                    tmpSpanWrite = tmpSpanWrite[2..];
                }

                Write(address, tmpSpan);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tmp);
            }

        }

    }
}
