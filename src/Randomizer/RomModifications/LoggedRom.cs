using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BpsNet; // Still needed to store/apply base patch

namespace Randomizer.RomModifications
{
    /// <summary>
    /// An implementation of IRom that logs write operations to memory instead of a file.
    /// It can optionally store a base BPS patch.
    /// Logged writes can be saved as an IPS patch.
    /// It allows overlapping writes, where the latest write takes precedence,
    /// and merges adjacent writes.
    /// </summary>
    public sealed class LoggedRom : IRom, IDisposable
    {
        private byte[]? _basePatchData; // Stored base BPS patch data

        // Store writes sorted by address. Key is address, Value is data.
        private readonly SortedDictionary<int, byte[]> _writes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggedRom"/> class.
        /// </summary>
        public LoggedRom()
        {
            // Constructor is parameterless
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
        /// Logs a write operation. Overlapping writes are allowed; the latest write
        /// takes precedence over the overlapping region. Merges adjacent writes.
        /// </summary>
        /// <param name="address">The starting address of the write.</param>
        /// <param name="data">The data to write.</param>
        public void Write(Address address, in ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty) return;

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
                if (_writes.TryGetValue(key, out var existingData))
                {
                    _writes.Remove(key); // Remove the overlapping block completely for now

                    int existingStart = key;
                    int existingEnd = existingStart + existingData.Length;

                    // Part before the new write starts?
                    if (existingStart < newStart)
                    {
                        int beforeLength = newStart - existingStart;
                        var beforeData = new byte[beforeLength];
                        Buffer.BlockCopy(existingData, 0, beforeData, 0, beforeLength);
                        nonOverlappingParts.Add(new KeyValuePair<int, byte[]>(existingStart, beforeData));
                    }

                    // Part after the new write ends?
                    if (existingEnd > newEnd)
                    {
                        int afterStartOffset = newEnd - existingStart;
                        int afterLength = existingEnd - newEnd;
                        var afterData = new byte[afterLength];
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
            KeyValuePair<int, byte[]> precedingEntry = _writes.LastOrDefault(kvp => kvp.Key < currentAddress);
            if (precedingEntry.Value != null && precedingEntry.Key + precedingEntry.Value.Length == currentAddress)
            {
                // Merge with preceding block
                var mergedData = new byte[precedingEntry.Value.Length + currentData.Length];
                Buffer.BlockCopy(precedingEntry.Value, 0, mergedData, 0, precedingEntry.Value.Length);
                Buffer.BlockCopy(currentData, 0, mergedData, precedingEntry.Value.Length, currentData.Length);

                currentAddress = precedingEntry.Key; // New effective start address
                currentData = mergedData; // New data is the merged data
                _writes.Remove(precedingEntry.Key); // Remove the old preceding entry
            }

            // 4. Merge with succeeding block if adjacent
            // Check if a block starts exactly where the current (potentially merged) block ends
            int currentEndAddress = currentAddress + currentData.Length;
            if (_writes.TryGetValue(currentEndAddress, out var succeedingData))
            {
                // Merge with succeeding block
                var mergedData = new byte[currentData.Length + succeedingData.Length];
                Buffer.BlockCopy(currentData, 0, mergedData, 0, currentData.Length);
                Buffer.BlockCopy(succeedingData, 0, mergedData, currentData.Length, succeedingData.Length);

                currentData = mergedData; // New data is the merged data
                _writes.Remove(currentEndAddress); // Remove the old succeeding entry
            }

            // 5. Add the final (potentially merged) new entry
            _writes[currentAddress] = currentData;
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
                // It's often better practice to let exceptions propagate or rethrow specific ones
                // depending on the application's error handling strategy.
                // For now, returning false matches the original behavior.
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
            // Ensure we use a BinaryWriter that doesn't close the underlying stream upon disposal
            using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true);

            // IPS Header "PATCH"
            writer.Write((byte)'P');
            writer.Write((byte)'A');
            writer.Write((byte)'T');
            writer.Write((byte)'C');
            writer.Write((byte)'H');

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
                    writer.Write((byte)((offset >> 16) & 0xFF));
                    writer.Write((byte)((offset >> 8) & 0xFF));
                    writer.Write((byte)(offset & 0xFF));

                    // Size (Big Endian)
                    writer.Write((byte)((recordLength >> 8) & 0xFF));
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
            writer.Write((byte)'E');
            writer.Write((byte)'O');
            writer.Write((byte)'F');
            // Flush the writer to ensure data is written to the stream before returning
            writer.Flush();
        }

        /// <summary>
        /// Disposes resources (no-op for LoggedRom as it holds no unmanaged resources directly).
        /// </summary>
        public void Dispose()
        {
            // No managed resources like streams opened by this class instance itself.
            // _basePatchData is just a byte array, handled by GC.
            // _writes dictionary keys/values are managed types.
            GC.SuppressFinalize(this);
        }
    }
}
