namespace Randomizer.Games.Zelda1;

using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Graph;
using static Randomizer.Games.Zelda1.YamlReader;

/// <summary>
/// Assigns per-screen cave IDs so shuffled shops don't all share the same backing data.
///
/// Z1 indexes cave contents by cave ID (ObjType+1 - 0x6A) and many screens reuse the same ID, so
/// without this every entrance to "cave 0x1A" shows the same shop. The ASM cave tables were extended
/// to 48 entries (IDs 0x10-0x3F); IDs 0x24-0x3F are free and all map to object types >= 0x7B, which
/// the engine empties after a single take/buy — so those IDs give single-purchase shops for free.
///
/// A rolled shop becomes either a buy-once shop (fresh ID >= 0x24, one item, progression-safe — the
/// engine empties it after the purchase) or stays a repeatable shop on its original ID < 0x24
/// (restocks on re-entry, so it's restricted to non-count stock and may be shared across screens).
/// </summary>
internal sealed class ShopShuffler
{
    // Free cave IDs available for unique single-purchase shops. All of these map to object types
    // >= 0x7B, which the engine empties after one purchase.
    private const int FirstFreeCaveId = 0x24;
    private const int LastFreeCaveId = 0x3F;

    // Flag/text bytes for a charging shop, matching the vanilla shop pattern (cave 0x1A). The
    // CaveFlags byte is assembled as: text&0xC0>>6 (PickItem|Shop) | flags[2]>>4 (ShowItems|
    // ShowPrices) | flags[1]>>2 (Money|Hint) | flags[0] (HeartReq|Neg). So a normal charging shop
    // sets flags[2]=0xC0 and the text byte's high bits to 0xC0; flags[0]/flags[1]=0.
    private const int ShopFlagShowByte = 0xC0;     // flags[2]: ShowItems + ShowPrices
    // 0xC0 (PickItem|Shop high bits) | 0x1C (text selector 0x1C = "BUY SOMETHIN' WILL YA," — the
    // vanilla generic shop line, used by caves 0x1D/0x1E). The moblin shopkeeper sprite for these
    // extended cave IDs is handled by the ClampCaveTypeAndCompare ASM hook.
    private const int ShopTextByte = 0xDC;

    // Fraction of shops made single-item buy-once (the rest stay repeatable). Buy-once shops are the
    // only ones that can safely sell count/progression items.
    private const double BuyOnceRatio = 1.0 / 3.0;

    private readonly PRNG _prng;
    private readonly YamlData _data;

    public ShopShuffler(PRNG prng, YamlData data)
    {
        _prng = prng;
        _data = data;
    }

    /// <summary>
    /// Reassign cave IDs and synthesize cave data so shops are unique. Mutates
    /// <see cref="OverworldMap.cave"/> for affected screens and adds new <see cref="CaveData"/>
    /// entries to <see cref="YamlData.caves"/>.
    /// </summary>
    public void Shuffle()
    {
        var freeIds = new Queue<int>(Enumerable.Range(FirstFreeCaveId, LastFreeCaveId - FirstFreeCaveId + 1));
        var cavesById = _data.caves.ToDictionary(c => c.cave);

        // Shuffleable shop screens. A screen is eligible only if it actually produces a graph cave
        // entrance: many screens carry a shop cave ID but model no cave node (unreachable/secret
        // duplicates), and reassigning those would desync logic from the ROM. (Take-anys are left
        // shared; they're junk-only and harmless.)
        var shopScreens = _data.overworld_maps
            .Where(m => m.name != "Meta"
                        && cavesById.TryGetValue(m.cave, out var c) && IsShop(c)
                        && ScreenHasCaveEntrance(m))
            .OrderBy(m => m.map) // deterministic order; PRNG decides the buy-once/repeatable split
            .ToList();

        // Roll each eligible shop screen buy-once or repeatable:
        //   - Buy-once: gets its own fresh cave ID >= 0x24 (the engine empties these after one
        //     purchase), making it single-item, single-purchase, and progression-safe.
        //   - Repeatable: keeps its original ID < 0x24, which restocks on re-entry (the fresh IDs
        //     don't); may be shared across screens.
        // Free IDs (28) far outnumber shop screens (~20), so every buy-once roll succeeds.
        foreach (var map in shopScreens)
        {
            bool buyOnce = _prng.GetRandomInt(0, 1000) < BuyOnceRatio * 1000;
            if (buyOnce && freeIds.Count > 0)
            {
                int newId = freeIds.Dequeue();
                _data.caves.Add(MakeBuyOnceShop(newId));
                map.cave = newId;
            }
            // else: repeatable — leave map.cave on its original shared ID < 0x24.
        }
    }

    // Mirrors BuildOverworldMap's cave-connect condition: a screen produces a "Cave XX - Entrance"
    // node only if it has a cave node of a standard entrance type (or is a secret[0] cave). Screens
    // that carry a shop cave ID but model no such node are unreachable duplicates and must not be
    // reassigned (their ROM shop would diverge from logic). This MUST stay in sync with
    // BuildOverworldMap, including the second-quest-only exclusion.
    private bool ScreenHasCaveEntrance(OverworldMap map)
    {
        // Second-quest-only hidden entrances (secret[1] == 1, secret[0] == 0) are not wired into the
        // first-quest graph, so they never produce a cave node. See BuildOverworldMap.
        if (map.secret[1] == 1 && map.secret[0] == 0)
            return false;

        var screen = _data.overworld_screens
            .FirstOrDefault(s => s.area == map.area && s.screen == map.screen);
        var cave = screen?.nodes.caves?.FirstOrDefault();
        if (cave == null)
            return false;
        return cave.type is CaveType.Open or CaveType.Push or CaveType.Bomb or CaveType.Tree or CaveType.Grave
            || map.secret[0] == 1;
    }

    // A cave is a shop if it has the Shop flag and shows items, and isn't a money game or hint.
    private static bool IsShop(CaveData c) =>
        c.Flag.HasFlag(CaveFlags.Shop)
        && c.Flag.HasFlag(CaveFlags.ShowItems)
        && !c.Flag.HasFlag(CaveFlags.MoneyGame)
        && !c.Flag.HasFlag(CaveFlags.Hint);

    // Placeholder item id marking an "active" shop slot. BuildCaves creates a graph location for any
    // slot whose item != 0x2F; the placement pass then overwrites the byte with the real item. The
    // value itself is never shown (it's replaced before the ROM is finalized).
    private const int ActiveSlotPlaceholder = 0x18; // rupee

    // A single-item charging shop in the middle slot. Only the middle slot is active so the player
    // can't take a wrong item and strand progression. The free ID (object type >= 0x7B) makes it
    // single-purchase, so it can safely hold count/progression items.
    private static CaveData MakeBuyOnceShop(int caveId) => new()
    {
        name = $"Cave {caveId:X2} (buy-once)",
        cave = caveId,
        items = [0x2F, ActiveSlotPlaceholder, 0x2F], // middle slot active; others empty
        flags = [0x00, 0x00, ShopFlagShowByte],      // ShowItems|ShowPrices (global shop flags)
        prices = [0x00, 0x00, 0x00],                 // written by WriteCavePrices
        text = ShopTextByte,                          // PickItem|Shop + shopkeeper text
        buyOnce = true,
    };
}
