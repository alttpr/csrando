namespace Randomizer.RomModifications;

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

/// <summary>
/// An implementation of a rom that logs write operations to memory.
/// It can optionally store a base BPS patch.
/// Logged writes can be saved as an IPS patch.
/// It allows overlapping writes, where the latest write takes precedence,
/// and merges adjacent writes.
/// </summary>
public sealed class LoggedRom : IRom
{
    // Store writes sorted by address. Key is address, Value is data.
    private readonly SortedDictionary<int, byte[]> _writes = [];

    public LoggedRom() { }

    /// <summary>
    /// Gets a read-only view of the logged writes.
    /// </summary>
    public IReadOnlyDictionary<int, byte[]> Writes => _writes;

    /// <summary>
    /// Gets the base BPS patch data that was applied, if any.
    /// </summary>
    public byte[]? BasePatchData { get; private set; }

    /// <summary>
    /// Logs a write operation. Overlapping writes are allowed; the latest write
    /// takes precedence over the overlapping region. Merges adjacent writes.
    /// </summary>
    /// <param name="address">The starting address of the write.</param>
    /// <param name="data">The data to write.</param>
    public void Write(Address address, in ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return;

        int newStart = address.Value;
        int newLength = data.Length;
        int newEnd = newStart + newLength; // Exclusive end address

        // 1. Identify and process overlapping/affected existing blocks
        // Find keys of blocks that overlap with the new write range [newStart, newEnd)
        var affectedKeys = _writes.Keys
            .Where(key =>
            {
                int existingStart = key;
                int existingLength = _writes[key].Length;
                int existingEnd = existingStart + existingLength;
                // Overlap condition: max(start1, start2) < min(end1, end2)
                return Math.Max(newStart, existingStart) < Math.Min(newEnd, existingEnd);
            })
            .ToList(); // Materialize keys to avoid modification issues during iteration

        // Store parts of affected blocks that *don't* overlap with the new write
        var nonOverlappingParts = new List<KeyValuePair<int, byte[]>>();

        foreach (int key in affectedKeys)
        {
            if (_writes.TryGetValue(key, out byte[]? existingData))
            {
                _writes.Remove(key); // Remove the overlapping block completely for now

                int existingStart = key;
                int existingEnd = existingStart + existingData.Length;

                // Part before the new write starts?
                if (existingStart < newStart)
                {
                    int beforeLength = newStart - existingStart;
                    byte[] beforeData = new byte[beforeLength];
                    Buffer.BlockCopy(existingData, 0, beforeData, 0, beforeLength);
                    nonOverlappingParts.Add(new KeyValuePair<int, byte[]>(existingStart, beforeData));
                }

                // Part after the new write ends?
                if (existingEnd > newEnd)
                {
                    int afterStartOffset = newEnd - existingStart;
                    int afterLength = existingEnd - newEnd;
                    byte[] afterData = new byte[afterLength];
                    Buffer.BlockCopy(existingData, afterStartOffset, afterData, 0, afterLength);
                    nonOverlappingParts.Add(new KeyValuePair<int, byte[]>(newEnd, afterData));
                }
            }
        }

        // Add back the non-overlapping parts
        foreach (var part in nonOverlappingParts)
        {
            // Could potentially re-overlap if the original affected blocks were adjacent,
            // but adding directly to SortedDictionary handles this key-wise.
            // We will merge adjacent blocks later anyway.
            _writes[part.Key] = part.Value;
        }

        // 2. Prepare the new data block
        byte[] newDataArray = data.ToArray();
        int currentAddress = newStart;
        byte[] currentData = newDataArray;

        // 3. Merge with preceding block if adjacent
        // Find the block that immediately precedes the current block *after* overlap handling
        var precedingEntry = _writes.LastOrDefault(kvp => kvp.Key < currentAddress);
        if (precedingEntry.Value != null && precedingEntry.Key + precedingEntry.Value.Length == currentAddress)
        {
            // Merge with preceding block
            byte[] mergedData = new byte[precedingEntry.Value.Length + currentData.Length];
            Buffer.BlockCopy(precedingEntry.Value, 0, mergedData, 0, precedingEntry.Value.Length);
            Buffer.BlockCopy(currentData, 0, mergedData, precedingEntry.Value.Length, currentData.Length);

            currentAddress = precedingEntry.Key; // New effective start address
            currentData = mergedData; // New data is the merged data
            _writes.Remove(precedingEntry.Key); // Remove the old preceding entry
        }

        // 4. Merge with succeeding block if adjacent
        // Check if a block starts exactly where the current (potentially merged) block ends
        int currentEndAddress = currentAddress + currentData.Length;
        if (_writes.TryGetValue(currentEndAddress, out byte[]? succeedingData))
        {
            // Merge with succeeding block
            byte[] mergedData = new byte[currentData.Length + succeedingData.Length];
            Buffer.BlockCopy(currentData, 0, mergedData, 0, currentData.Length);
            Buffer.BlockCopy(succeedingData, 0, mergedData, currentData.Length, succeedingData.Length);

            currentData = mergedData; // New data is the merged data
            _writes.Remove(currentEndAddress); // Remove the old succeeding entry
        }

        // 5. Add the final (potentially merged) new entry
        _writes[currentAddress] = currentData;
    }

    /// <summary>
    /// Reads data from the logged writes. Simulates the state after all writes
    /// have been applied.
    /// </summary>
    /// <param name="address">The starting address to read from.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>A byte array containing the data read from the logged writes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if length is negative.</exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if any part of the requested address range [address, address + length)
    /// has not been written to by any logged write operation. Reading from LoggedRom
    /// requires the entire requested range to have been previously written.
    /// </exception>
    public byte[] Read(Address address, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
        if (length == 0)
            return [];

        byte[] result = new byte[length];
        int readStart = address.Value;

        int currentResultIndex = 0;
        while (currentResultIndex < length)
        {
            int targetAddress = readStart + currentResultIndex;

            // Find the block that contains targetAddress.
            // Since writes are merged and overlaps resolved, there will be at most one such block.
            // We look for the block with the largest key <= targetAddress.
            var candidate = _writes.LastOrDefault(kvp => kvp.Key <= targetAddress);

            // Check if the candidate block actually covers the targetAddress
            if (candidate.Value == null || // No block starts at or before targetAddress
                candidate.Key + candidate.Value.Length <= targetAddress) // Block ends before or at targetAddress
            {
                // If no block covers the *start* of the remaining read segment, the read fails.
                throw new KeyNotFoundException($"Logged write data not found for address 0x{targetAddress:X}. Reading from LoggedRom requires the entire requested range [0x{readStart:X}-0x{readStart + length:X}) to have been previously written.");
            }

            // The candidate block covers targetAddress.
            int blockStart = candidate.Key;
            byte[] blockData = candidate.Value;

            // Calculate offset *within the source block data* where our targetAddress lies
            int sourceOffset = targetAddress - blockStart;

            // Calculate how many bytes we can copy *from this block*, starting from sourceOffset
            int bytesAvailableInBlock = blockData.Length - sourceOffset;

            // Calculate how many bytes we still *need* for the result array
            int bytesNeededForResult = length - currentResultIndex;

            // Determine how many bytes to copy in this step (the minimum of the two)
            int bytesToCopy = Math.Min(bytesAvailableInBlock, bytesNeededForResult);

            // Copy the data chunk from the blockData into the result array
            Buffer.BlockCopy(blockData, sourceOffset, result, currentResultIndex, bytesToCopy);

            // Advance the index for the result buffer
            currentResultIndex += bytesToCopy;
        }

        return result;
    }

    /// <summary>
    /// Resizing a LoggedRom is not supported. This operation is ignored.
    /// </summary>
    public void Resize(int size)
    {
        // This operation doesn't make sense in the context of logging writes for a patch.
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
        if (BasePatchData != null)
            throw new InvalidOperationException("ApplyBasePatch can only be called once.");
        if (!baseBPS.Exists)
            throw new FileNotFoundException("Base BPS patch file not found.", baseBPS.FullName);

        BasePatchData = File.ReadAllBytes(baseBPS.FullName);
    }

    /// <summary>
    /// Updating the checksum on a LoggedRom is not supported. This operation is ignored.
    /// </summary>
    public void UpdateChecksum()
    {
        // Updating the checksum on a LoggedRom is not supported.
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
        // Ensure we use a BinaryWriter that doesn't close the underlying stream upon disposal
        using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true);

        // IPS Header "PATCH"
        writer.Write("PATCH".AsSpan());

        // The SortedDictionary ensures writes are already sorted by address.
        foreach (var kvp in _writes)
        {
            int offset = kvp.Key;
            byte[] data = kvp.Value;
            int length = data.Length;

            // IPS format does not support records larger than 65535 bytes (0xFFFF).
            // Split large blocks into multiple IPS records.
            int dataIndex = 0;
            while (length > 0)
            {
                int recordLength = Math.Min(length, 0xFFFF); // Max IPS record size

                // Write IPS record (Offset: 3 bytes, Size: 2 bytes, Data: N bytes)
                // Offset (Big Endian)
                writer.Write((byte)(offset >> 16 & 0xFF));
                writer.Write((byte)(offset >> 8 & 0xFF));
                writer.Write((byte)(offset & 0xFF));

                // Size (Big Endian)
                writer.Write((byte)(recordLength >> 8 & 0xFF));
                writer.Write((byte)(recordLength & 0xFF));

                // Data
                writer.Write(data, dataIndex, recordLength);

                // Update offset and length for the next chunk if needed
                offset += recordLength;
                dataIndex += recordLength;
                length -= recordLength;
            }
        }

        // IPS Footer "EOF"
        writer.Write("EOF".AsSpan());
        // Flush the writer to ensure data is written to the stream before returning
        writer.Flush();
    }

    /// <summary>
    /// Disposes resources (no-op for LoggedRom as it holds no unmanaged resources directly).
    /// </summary>
    public void Dispose()
    {
        // No managed resources like streams opened by this class instance itself.
        // BasePatchData is just a byte array, handled by GC.
        // Writes dictionary keys/values are managed types.
        GC.SuppressFinalize(this);
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
