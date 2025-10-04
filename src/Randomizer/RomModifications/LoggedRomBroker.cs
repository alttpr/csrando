namespace Randomizer.RomModifications;

using Randomizer.Graph;

public sealed class LoggedRomBroker : IRomBroker
{
    private readonly Dictionary<string, (byte[]?, byte[])> _worlds = [];
    public IReadOnlyDictionary<string, (byte[]? BpsPatch, byte[] IpsPatch)> Worlds => _worlds.AsReadOnly();

    public IRom CreateRom(GameRandomizer randomizer)
    {
        return new LoggedRom();
    }

    public void SaveRom(IRom rom, string suggestedFileName)
    {
        if (rom is not LoggedRom loggedRom)
        {
            return;
        }

        _worlds[Path.GetFileNameWithoutExtension(suggestedFileName)] = (loggedRom.BasePatchData, loggedRom.GetIpsPatchData());
    }
}
