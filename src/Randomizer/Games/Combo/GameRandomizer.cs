namespace Randomizer.Games.Combo;

using Randomizer.Games.Alttp;
using Randomizer.Graph;
using Randomizer.RomModifications;
using System.Reflection;
using System.Reflection.Metadata;
using BaseGameRandomizer = Graph.GameRandomizer;

public sealed class GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng)
    : BaseGameRandomizer(randomizerConfigs, prng)
{
    private const int RomSize = 8 * 1024 * 1024;

    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng) => new ItemPooler(worlds, prng);

    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(SpoilerLog spoilerLog) { } // FIXME: implement a spoiler log

    protected override void WriteWorldToRom(IWorld world, IRom rom, PRNG prng)
    {
        var comboWorld = world as World;
        if (comboWorld == null)
            throw new ArgumentException("Passed world is not for the Combo Randomizer.", nameof(world));

        // Use the concrete instance for combo-specific methods
        WriteItemsToRom(comboWorld, rom);

        // Pass the original IRom (could be Rom or LoggedRom) to individual game writers
        if (comboWorld.AlttpWorld != null)
            Alttp.RomWriter.Write(rom, comboWorld.AlttpWorld, prng, 0x400000);

        if (comboWorld.Z1World != null)
            Zelda1.RomWriter.Write(rom, comboWorld.Z1World, prng);

        if (comboWorld.M1World != null)
            Metroid.RomWriter.Write(rom, comboWorld.M1World, prng);

        if (comboWorld.SMWorld != null)
            SuperMetroid.RomWriter.Write(rom, comboWorld.SMWorld, prng);

        // Use the concrete instance for combo-specific methods
        PortalWriter.WritePortals(rom, comboWorld);
        WriteGameFlags(comboWorld, rom);
        WriteSeed(comboWorld, rom);
        WriteComboVersionStrings(rom);
    }

    private void WriteSeed(World world, IRom rom)
    {
        rom.Write(0x7ffff0, BitConverter.GetBytes(world.Prng.Seed));
    }

    private void WriteGameFlags(World world, IRom rom)
    {
        byte startingGame = world.Config.InitialGame switch
        {
            "sm" => 0x00,
            "alttp" => 0x01,
            "z1" => 0x02,
            "m1" => 0x03,
            "" => (byte)(world.SMWorld != null ? 0x00 : world.AlttpWorld != null ? 0x01 : world.Z1World != null ? 0x02 : world.M1World != null ? 0x03 : 0x00),
            _ => throw new ArgumentException("Invalid initial game", nameof(world.Config.InitialGame))
        };

        rom.Write(0x7fffe0, [startingGame]);
        rom.Write(0x7fffe2, [(byte)(world.SMWorld == null ? 0x00 : 0x01)]);
        rom.Write(0x7fffe4, [(byte)(world.AlttpWorld == null ? 0x00 : 0x01)]);
        rom.Write(0x7fffe6, [(byte)(world.Z1World == null ? 0x00 : 0x01)]);
        rom.Write(0x7fffe8, [(byte)(world.M1World == null ? 0x00 : 0x01)]);
    }

    private void WriteItemsToRom(World world, IRom rom)
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
            else if (location.World is SuperMetroid.World)
            {
                // Added null check for Addresses
                if (location.Addresses == null) continue;

                int plmBytes = (int)itemBytes[0] + ((int)itemBytes[1] << 8);
                int offset = ((SuperMetroid.Vertex)location).Node!.NodeSubType switch
                {
                    "chozo" => plmBytes >= 0xEFE0 ? 0x04 : 0x54,
                    "hidden" => plmBytes >= 0xEFE0 ? 0x08 : 0xA8,
                    _ => 0
                };

                plmBytes += offset;
                rom.Write((Address)location.Addresses[0], [(byte)(plmBytes & 0xFF), (byte)((plmBytes >> 8) & 0xFF)]);

                if (plmBytes >= 0xEFE0)
                {
                    rom.Write((Address)(location.Addresses[0] + 5), [itemBytes[2]]);
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

    public void WriteComboVersionStrings(IRom rom)
    {
        // Get the current Git commit hash
        string commitHash = ThisAssembly.Git.Commit;
        var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
        string versionString = $"Quad v.{version.Major}.{version.Minor}.{version.Build} S{prng.Seed:X08}";
        string commitString = $"{DateTime.Now.ToShortDateString()} - #{commitHash}".PadLeft(26, ' ');

        rom.Write(0x7C0001, ConvertStringToByteArray(versionString));
        rom.Write(0x7C001D, ConvertStringToByteArray(commitString));
        rom.Write(0x7FFFF0, BitConverter.GetBytes(prng.Seed));

    }

    protected override string CreateFileName(IWorld world, PRNG prng, string? worldSuffix)
        => $"combo_{prng.Seed:x08}{worldSuffix}.sfc";

    private static readonly Dictionary<char, byte> charToByteMap = new Dictionary<char, byte>
    {
        {'A', 0x50}, {'a', 0x50}, {'B', 0x51}, {'b', 0x51}, {'C', 0x52}, {'c', 0x52},
        {'D', 0x53}, {'d', 0x53}, {'E', 0x54}, {'e', 0x54}, {'F', 0x55}, {'f', 0x55},
        {'G', 0x56}, {'g', 0x56}, {'H', 0x57}, {'h', 0x57}, {'I', 0x58}, {'i', 0x58},
        {'J', 0x59}, {'j', 0x59}, {'K', 0x5A}, {'k', 0x5A}, {'L', 0x5B}, {'l', 0x5B},
        {'M', 0x5C}, {'m', 0x5C}, {'N', 0x5D}, {'n', 0x5D}, {'O', 0x5E}, {'o', 0x5E},
        {'P', 0x5F}, {'p', 0x5F}, {'Q', 0x60}, {'q', 0x60}, {'R', 0x61}, {'r', 0x61},
        {'S', 0x62}, {'s', 0x62}, {'T', 0x63}, {'t', 0x63}, {'U', 0x64}, {'u', 0x64},
        {'V', 0x65}, {'v', 0x65}, {'W', 0x66}, {'w', 0x66}, {'X', 0x67}, {'x', 0x67},
        {'Y', 0x68}, {'y', 0x68}, {'Z', 0x69}, {'z', 0x69}, {':', 0x4A}, {'!', 0x6A},
        {'.', 0x6B}, {'-', 0x6C}, {',', 0x6D}, {'?', 0x6E}, {'#', 0x6F}, {' ', 0x1F},
        {'0', 0x70}, {'1', 0x71}, {'2', 0x72}, {'3', 0x73}, {'4', 0x74}, {'5', 0x75},
        {'6', 0x76}, {'7', 0x77}, {'8', 0x78}, {'9', 0x79}, {'\'', 0x7E}, {'(', 0x88},
        {')', 0x89}, {'%', 0x8A}, {'+', 0x8B}
    };

    public static byte[] ConvertStringToByteArray(string input)
    {
        return input.Select(c => charToByteMap.TryGetValue(c, out byte value) ? value : (byte)0)
                    .ToArray();
    }
}
