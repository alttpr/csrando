namespace Randomizer.RomModifications;

using System;
using System.IO;

public interface IRom : IDisposable
{
    void Resize(int size);
    void ApplyBasePatch(FileInfo baseBPS);
    void UpdateChecksum();
    bool Save(string outputLocation);
    void Write(Address address, in ReadOnlySpan<byte> data);
    byte[] Read(Address address, int length);
}
