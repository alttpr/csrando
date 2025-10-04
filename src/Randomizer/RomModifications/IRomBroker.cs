namespace Randomizer.RomModifications;

using Randomizer.Graph;

public interface IRomBroker
{
    IRom CreateRom(GameRandomizer randomizer);
    void SaveRom(IRom rom, string suggestedFileName);
}
