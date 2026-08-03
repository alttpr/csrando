namespace Randomizer.Games.Metroid;

using Randomizer.Graph;
using Randomizer.RomModifications;

public static class RomWriter
{
    /// <summary>
    /// PC address of the cross-game transition table (SNES $98:F000, transition_tables.asm).
    /// The combo PortalWriter rewrites it; standalone seeds terminate it.
    /// </summary>
    public const int TransitionTableAddress = 0x6C7000;

    /// <summary>
    /// PC address of config_m1_start_area (SNES $98:FF00, config.asm): the area the
    /// boot code cold-starts into, in InArea order (0 Brinstar .. 4 Ridley). The
    /// per-area orientation table follows at +1 and keeps its vanilla defaults as long
    /// as the start cells stay vanilla.
    /// </summary>
    public const int StartAreaConfigAddress = 0x6C7F00;

    public static void Write(IRom baseRom, World world, PRNG prng, int offset = 0)
    {
        // Patch data first: under map shuffle it contains the generated item tables with
        // placeholder item bytes, which WriteItems then overwrites with the filled items.
        WritePatchData(baseRom, world, offset);
        WriteItems(baseRom, world, offset);
    }

    /// <summary>
    /// Writes only the patch data. The combo writer needs this before its own item pass,
    /// because the map-shuffle patch data contains the item tables the items land in.
    /// </summary>
    public static void WritePatchData(IRom baseRom, World world, int offset = 0) =>
        new Rom(baseRom, offset).WritePatchData(world);

    /// <summary>Writes the filled items for locations the combo item pass did not claim.</summary>
    public static void WriteItems(IRom baseRom, World world, int offset = 0) =>
        new Rom(baseRom, offset).WriteItems(world);
}
