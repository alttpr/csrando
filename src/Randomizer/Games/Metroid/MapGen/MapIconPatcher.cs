namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Linq;
using Randomizer.Graph;

/// <summary>
/// Post-fill pass that swaps automap item glyphs for their energy/major variants
/// based on the filled items. The automap itself is composed during map generation,
/// before any item is known, so this runs from <see cref="Rom.WritePatchData"/> —
/// after the filler, while the patch data is still in memory. Works on both layouts:
/// generated maps get their composed planes rewritten in place (and the seed id
/// rehashed to match), vanilla layouts get word patches over the base ROM's payload.
/// </summary>
public static class MapIconPatcher
{
    /// <summary>
    /// The character every vanilla item cell renders with: the east+west connection
    /// variant of the item glyph, no flips (verified against m1_map_vanilla_tilemaps.bin,
    /// where each item coordinate holds the word $2D09).
    /// </summary>
    private const int VanillaItemCharacter = 0x109;

    public static void Apply(World world)
    {
        var resolver = ItemTiers.CreateResolver(world.Config.TieredItems, world.Config.CustomItemTiers);
        if (resolver == null || world.PatchData == null)
            return;

        var tiles = AutomapTiles.Load();
        if (world.GeneratedMap != null)
            ApplyToGeneratedPlanes(world, tiles, resolver);
        else
            ApplyToVanillaPlanes(world, tiles, resolver);
    }

    private static void ApplyToGeneratedPlanes(World world, AutomapTiles tiles, Func<IItem, ItemTier> resolver)
    {
        var generated = world.GeneratedMap!;
        if (generated.ItemAddresses == null
            || !world.PatchData!.TryGetValue(AutomapComposer.TilemapsAddress, out var planes))
        {
            throw new InvalidOperationException("generated map has no automap payload to patch");
        }

        var cellByAddress = generated.ItemAddresses.ToDictionary(kv => (long)kv.Value, kv => kv.Key);
        bool changed = false;
        foreach (var location in world.GetLocationsOfType(VertexType.Item))
        {
            if (location.Item == null || location.Addresses is not [var address, ..])
                continue;
            if (!cellByAddress.TryGetValue(address, out var point))
                continue;

            var cell = generated.Grid.Cell(point)
                ?? throw new InvalidOperationException($"item address maps to empty cell {point}");
            int offset = (int)cell.Area * tiles.BytesPerArea + (point.Y * WorldGrid.Size + point.X) * 2;
            ushort word = (ushort)(planes[offset] | planes[offset + 1] << 8);
            int character = word & tiles.CharacterMask;

            // The composer drew the cell with the plain item glyph; find its position
            // in whichever glyph group it currently uses so a repeated pass stays
            // idempotent instead of compounding. Cells drawn with a non-item glyph
            // (a boss room holding an item) have no tiered variant and keep theirs.
            if (ItemGlyphIndex(tiles, character) is not int index)
                continue;

            int tieredBase = resolver(location.Item) switch
            {
                ItemTier.Major => tiles.MajorTileBase,
                ItemTier.Medium => tiles.EnergyTileBase,
                _ => tiles.ItemTileBase,
            };
            ushort patched = (ushort)(word & ~tiles.CharacterMask | (tieredBase + index));
            if (patched == word)
                continue;

            planes[offset] = (byte)patched;
            planes[offset + 1] = (byte)(patched >> 8);
            changed = true;
        }

        // The runtime compares the stamped seed id against SRAM to reset stale
        // explored-map state; keep it a hash of the payload actually written.
        if (changed && world.PatchData.TryGetValue(AutomapComposer.BoundsAddress, out var bounds))
        {
            uint seedId = AutomapComposer.SeedHash(planes, bounds);
            world.PatchData[AutomapComposer.SeedIdAddress] =
                [(byte)seedId, (byte)(seedId >> 8), (byte)(seedId >> 16), (byte)(seedId >> 24)];
        }
    }

    /// <summary>The character's index within its item/energy/major glyph group, or null.</summary>
    private static int? ItemGlyphIndex(AutomapTiles tiles, int character)
    {
        foreach (int groupBase in (int[])[tiles.ItemTileBase, tiles.EnergyTileBase, tiles.MajorTileBase])
        {
            if (character >= groupBase && character < groupBase + tiles.ItemTileCount)
                return character - groupBase;
        }
        return null;
    }

    private static void ApplyToVanillaPlanes(World world, AutomapTiles tiles, Func<IItem, ItemTier> resolver)
    {
        foreach (var location in world.GetLocationsOfType(VertexType.Item))
        {
            if (location.Item == null || location.Addresses is not [var address, ..])
                continue;
            var tier = resolver(location.Item);
            if (tier == ItemTier.Minor)
                continue;
            if (!DataLoader.TryGetVanillaItemCell(address, out var area, out var cell))
                continue;

            int index = VanillaItemCharacter - tiles.ItemTileBase;
            int character = (tier == ItemTier.Major ? tiles.MajorTileBase : tiles.EnergyTileBase) + index;
            ushort word = (ushort)(character | tiles.Attributes);
            int patchAddress = AutomapComposer.TilemapsAddress
                + (int)area * tiles.BytesPerArea + (cell.Y * WorldGrid.Size + cell.X) * 2;
            world.PatchData![patchAddress] = [(byte)word, (byte)(word >> 8)];
        }
    }
}
