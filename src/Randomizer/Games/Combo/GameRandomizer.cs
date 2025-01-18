namespace Randomizer.Games.Combo;

using Randomizer.Graph;
using Randomizer.RomModifications;
using static Randomizer.Games.Metroid.YamlReader;
using BaseGameRandomizer = Graph.GameRandomizer;

public sealed class GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng) : BaseGameRandomizer(randomizerConfigs, prng)
{
    private const int RomSize = 8 * 1024 * 1024;

    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng) => new ItemPooler(worlds, prng);

    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(SpoilerLog spoilerLog) { } // FIXME: implement a spoiler log

    protected override RomModifications.Rom CreateRom(FileInfo baseRom, FileInfo? baseBPS)
    {
        var rom = new RomModifications.Rom(baseRom.FullName);
        // TODO: check hash? do we need that?

        // assume we either have a vanilla rom and a BPS, or an already pre-patched base rom.
        if (baseBPS != null)
        {
            rom.Resize(RomSize);
            rom.ApplyBasePatch(baseBPS);
        }

        return rom;
    }

    protected override void WriteWorldToRom(IWorld world, RomModifications.Rom rom, PRNG prng)
    {
        var comboWorld = world as World;
        if(comboWorld == null)
            throw new ArgumentException("Passed world is not for the Combo Randomizer.", nameof(world));

        WriteItemsToRom(comboWorld, rom);

        if (comboWorld.AlttpWorld != null)
            Alttp.RomWriter.Write(rom, comboWorld.AlttpWorld, prng, 0x400000);

        if (comboWorld.Z1World != null)
            Zelda1.RomWriter.Write(rom, comboWorld.Z1World, prng);

        if (comboWorld.M1World != null)
            Metroid.RomWriter.Write(rom, comboWorld.M1World, prng);

        // Set ALTTP as startup ROM
        rom.Write(0x7FFFE0, [2]);
    }

    private void WriteItemsToRom(World world, RomModifications.Rom rom)
    {
        // Replace all the item bytes to write with the combo specific item bytes depending on target game and write it to the rom
        var itemLocations = world.GetLocationsOfType(VertexType.Item).Where(x => x.Item != null);
        foreach (var location in itemLocations)
        {
            var itemBytes = ItemMapper.GetItemBytes(location, location.Item!);
            if (itemBytes == null)
            {
                continue;
            }

            var addresses = location.Addresses;
            if (addresses == null)
            {
                continue;
            }

            if (location.World is Alttp.World)
            {
                for (int i = 0; i < Math.Min(itemBytes.Length, location.Addresses!.Length); i++)
                {
                    if (i >= location.Addresses.Length)
                        break;
                    long address = location.Addresses[i];
                    byte? itemByte = itemBytes.ElementAtOrDefault(i);
                    if (itemByte == null)
                        continue;

                    var pcAddress = (Address)((SNES)address);
                    rom.Write((Address)(pcAddress.Value + 0x400000), [itemByte.Value]);
                }
            }
            else
            {
                rom.Write((Address)addresses[0], itemBytes);
            }

            // Clear out address so the item isn't written by the game specific writers with the wrong bytes
            location.Addresses = null;
        }
    }

    protected override string CreateFileName(IWorld world, PRNG prng, string? worldSuffix)
        => $"combo_{prng.Seed:x08}{worldSuffix}.sfc";
}
