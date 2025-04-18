namespace Randomizer.RomModifications;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BpsNet; // Still needed to store/apply base patch

/// <summary>
/// An implementation of IRom that logs write operations to memory instead of a file.
/// It can optionally store a base BPS patch.
/// Logged writes can be saved as an IPS patch.
/// It prevents overlapping writes and merges adjacent writes.
/// </summary>
public sealed class LoggedRom : IRom
{
    private byte[]? _basePatchData; // Stored base BPS patch data

    // Store writes sorted by address. Key is address, Value is data.
    private readonly SortedDictionary<int, byte[]> _writes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggedRom"/> class.
    /// </summary>
    public LoggedRom() // Removed baseRomPath parameter
    {
        // Constructor is now parameterless
    }

    /// <summary>
    /// Gets a read-only view of the logged writes.
    /// </summary>
    public IReadOnlyDictionary<int, byte[]> Writes => _writes;

    /// <summary>
    /// Gets the base BPS patch data that was applied, if any.
    /// </summary>
    public byte[]? BasePatchData => _basePatchData;

    /// <summary>
    /// Logs a write operation. Merges adjacent writes and prevents overlapping writes.
    /// </summary>
    /// <param name="address">The starting address of the write.</param>
    /// <param name="data">The data to write.</param>
    /// <exception cref="ArgumentException">Thrown if the write overlaps with existing logged data.</exception>
    public void Write(Address address, in ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return;

        int startAddress = address.Value;
        int endAddress = startAddress + data.Length; // Exclusive end address

        // Check for overlaps with existing writes
        foreach (var kvp in _writes)
        {
            int existingStart = kvp.Key;
            int existingEnd = existingStart + kvp.Value.Length;
            // Overlap conditions:
            // 1. New write starts within an existing write
            // 2. New write ends within an existing write
            // 3. New write completely envelops an existing write
            // 4. Existing write completely envelops the new write (covered by 1 & 2)
            if ((startAddress >= existingStart && startAddress < existingEnd) || // Starts inside
                (endAddress > existingStart && endAddress <= existingEnd) ||   // Ends inside
                (startAddress < existingStart && endAddress > existingEnd))      // Envelops
            {
                throw new ArgumentException($"Write at address 0x{startAddress:X} (length {data.Length}) overlaps with existing write at 0x{existingStart:X} (length {kvp.Value.Length})");
            }
        }

        byte[] newData = data.ToArray();
        int currentAddress = startAddress;

        // Check for merging with the preceding block
        KeyValuePair<int, byte[]> precedingEntry = default;
        foreach (var kvp in _writes.Reverse()) // More efficient to check backwards for preceding
        {
            if (kvp.Key < currentAddress)
            {
                if (kvp.Key + kvp.Value.Length == currentAddress)
                {
                    precedingEntry = kvp;
                }
                break; // Found the closest preceding entry (or the first one < currentAddress)
            }
        }

        if (precedingEntry.Value != null)
        {
            // Merge with preceding block
            var mergedData = new byte[precedingEntry.Value.Length + newData.Length];
            Buffer.BlockCopy(precedingEntry.Value, 0, mergedData, 0, precedingEntry.Value.Length);
            Buffer.BlockCopy(newData, 0, mergedData, precedingEntry.Value.Length, newData.Length);

            currentAddress = precedingEntry.Key; // New effective start address is the preceding block's start
            newData = mergedData; // New data is the merged data
            _writes.Remove(precedingEntry.Key); // Remove the old preceding entry
        }

        // Check for merging with the succeeding block
        if (_writes.TryGetValue(endAddress, out var succeedingData))
        {
            // Merge with succeeding block
            var mergedData = new byte[newData.Length + succeedingData.Length];
            Buffer.BlockCopy(newData, 0, mergedData, 0, newData.Length);
            Buffer.BlockCopy(succeedingData, 0, mergedData, newData.Length, succeedingData.Length);

            newData = mergedData; // New data is the merged data
            _writes.Remove(endAddress); // Remove the old succeeding entry
        }

        // Add the (potentially merged) new entry
        _writes[currentAddress] = newData;
    }

    /// <summary>
    /// Reading from a LoggedRom is not supported as it only tracks writes.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public byte[] Read(Address address, int length)
    {
        throw new NotSupportedException("Reading from a LoggedRom is not supported.");
    }

    /// <summary>
    /// Resizing a LoggedRom is not supported. This operation is ignored.
    /// </summary>
    public void Resize(int size)
    {
        // This operation doesn't make sense in the context of logging writes for a patch.
        // Consider logging a warning if this behavior is unexpected.
        // Console.WriteLine($"Warning: Resize({size}) called on LoggedRom, operation ignored.");
    }

    /// <summary>
    /// Stores the base BPS patch data. This patch will be combined with logged writes
    /// when generating the final output patch using Save or GetCombinedPatchData.
    /// </summary>
    /// <param name="baseBPS">The FileInfo object pointing to the base BPS patch file.</param>
    /// <exception cref="InvalidOperationException">Thrown if ApplyBasePatch is called more than once.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the baseBPS file does not exist.</exception>
    /// <exception cref="IOException">Thrown if an error occurs reading the baseBPS file.</exception>
    public void ApplyBasePatch(FileInfo baseBPS)
    {
        if (_basePatchData != null)
        {
            throw new InvalidOperationException("ApplyBasePatch can only be called once.");
        }
        if (!baseBPS.Exists)
        {
            throw new FileNotFoundException("Base BPS patch file not found.", baseBPS.FullName);
        }
        _basePatchData = File.ReadAllBytes(baseBPS.FullName);
    }

    /// <summary>
    /// Updating the checksum on a LoggedRom is not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public void UpdateChecksum()
    {
        throw new NotSupportedException("Updating the checksum on a LoggedRom is not supported.");
    }

    /// <summary>
    /// Saves the logged writes as an IPS patch file.
    /// Note: This does *not* include the base BPS patch data.
    /// </summary>
    /// <param name="outputLocation">The path to save the .ips file.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool Save(string outputLocation)
    {
        try
        {
            using var stream = new FileStream(outputLocation, FileMode.Create, FileAccess.Write);
            WriteIpsToStream(stream);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving IPS patch to {outputLocation}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Generates the IPS patch data for the logged writes and returns it as a byte array.
    /// Note: This does *not* include the base BPS patch data.
    /// </summary>
    /// <returns>A byte array containing the IPS patch data.</returns>
    public byte[] GetIpsPatchData()
    {
        using var memoryStream = new MemoryStream();
        WriteIpsToStream(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Writes the IPS patch data to the provided stream.
    /// </summary>
    /// <param name="stream">The stream to write the IPS patch data to.</param>
    private void WriteIpsToStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true); // Ensure writer doesn't close stream

        // IPS Header "PATCH"
        writer.Write((byte)'P');
        writer.Write((byte)'A');
        writer.Write((byte)'T');
        writer.Write((byte)'C');
        writer.Write((byte)'H');

        // Sort writes by address before generating patch
        var sortedWrites = _writes.OrderBy(kvp => kvp.Key);

        foreach (var kvp in sortedWrites)
        {
            int offset = kvp.Key;
            byte[] data = kvp.Value;
            int length = data.Length;

            // IPS format does not support records larger than 65535 bytes.
            // Split large records if necessary.
            int dataIndex = 0;
            while (length > 0)
            {
                int recordLength = Math.Min(length, 0xFFFF); // Max IPS record size

                // Write IPS record (Offset: 3 bytes, Size: 2 bytes, Data: N bytes)
                // Offset (Big Endian)
                writer.Write((byte)(offset >> 16));
                writer.Write((byte)(offset >> 8));
                writer.Write((byte)offset);

                // Size (Big Endian)
                writer.Write((byte)(recordLength >> 8));
                writer.Write((byte)recordLength);

                // Data
                writer.Write(data, dataIndex, recordLength);

                // Update offset and length for next chunk if needed
                offset += recordLength;
                dataIndex += recordLength;
                length -= recordLength;
            }
        }

        // IPS Footer "EOF"
        writer.Write((byte)'E');
        writer.Write((byte)'O');
        writer.Write((byte)'F');
    }


    /// <summary>
    /// Disposes resources (no-op for LoggedRom).
    /// </summary>
    public void Dispose()
    {
        // Nothing managed to dispose
        GC.SuppressFinalize(this);
    }
}
