namespace Randomizer.RomModifications;

using Randomizer.Graph;

public sealed class LoggedRomBroker : IRomBroker
{
    private readonly Dictionary<string, (byte[]?, byte[])> _worlds = [];
    public IReadOnlyDictionary<string, (byte[]? BpsPatch, byte[] IpsPatch)> Worlds => _worlds.AsReadOnly();

    public IRom CreateRom(GameRandomizer randomizer)
    {
        // TODO: this is only temporary because GameRom.Read exists (mostly for simplicity/laziness, not because it's needed).
        //       any time the LoggedRom can't return data, it'll read straight from the supplied base rom.
        //       if we decide to move everything to data (which is a reasonable chunk of work for room data), this can be removed along with the GameRom.Read method.
        if (randomizer.ProvideBaseRom() is { Exists: true } baseRom)
            return new LoggedRomWithFile(baseRom.FullName);

        return new LoggedRom();
    }

    public void SaveRom(IRom rom, string suggestedFileName)
    {
        if (rom is not LoggedRom loggedRom)
        {
            if (rom is not LoggedRomWithFile { LoggedRom: var lr })
                return;
            loggedRom = lr;
        }

        _worlds[Path.GetFileNameWithoutExtension(suggestedFileName)] = (loggedRom.BasePatchData, loggedRom.GetIpsPatchData());
    }

    private sealed class LoggedRomWithFile(string baseRomPath) : IRom
    {
        public LoggedRom LoggedRom { get; } = new();
        private readonly FileStream _rom = new(baseRomPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        void IRom.ApplyBasePatch(FileInfo baseBPS) => LoggedRom.ApplyBasePatch(baseBPS);
        void IRom.Resize(int size) => LoggedRom.Resize(size);
        void IRom.UpdateChecksum() => LoggedRom.UpdateChecksum();
        void IRom.Write(Address address, in ReadOnlySpan<byte> data) => LoggedRom.Write(address, data);
        void IDisposable.Dispose()
        {
            LoggedRom.Dispose();
            _rom.Dispose();
        }

        byte[] IRom.Read(Address address, int length)
        {
            try
            {
                return LoggedRom.Read(address, length);
            }
            catch (KeyNotFoundException) // NOTE: implementation detail of LoggedRom.Read, it throws that one when it has no data.
            {
                _rom.Seek(address.Value, SeekOrigin.Begin);
                var data = new byte[length];
                _rom.ReadExactly(data);
                return data;
            }
        }
    }
}
