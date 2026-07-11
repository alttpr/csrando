namespace RandomizerTests.Games.Zelda1;

using System.Collections.Generic;
using System.Linq;
using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;

/// <summary>
/// Overworld-shuffle landing positions. The flute (recorder_y_pos) is recomputed from the per-screen
/// walkability grids when dungeons move; cave/any-road exit[] stays at its vanilla per-screen value.
/// </summary>
[TestClass]
[TestCategory(TestCategories.Slow)]
public sealed class OverworldLandingTest
{
    private static bool[,]? WalkGrid(YamlReader.Screen? s) => OverworldWalkability.Parse(s);

    private static World BuildWorld(int seed)
    {
        var config = new Config { EntranceShuffle = EntranceShuffleOption.Overworld };
        return new World(0, new WorldConfig { Zelda1 = config }, new Graph(), new PRNG(seed));
    }

    [TestMethod]
    public void CaveAndAnyRoadExits_KeepVanillaPerScreenExit()
    {
        // exit[] belongs to the screen, and the shuffle only moves cave contents between screens, so
        // every shuffled exit[] must match its unshuffled baseline. Recomputing it (as an earlier
        // version did) breaks the X position and the walk-out animation.
        var baseline = new Config { EntranceShuffle = EntranceShuffleOption.None };
        var baseData = new World(0, new WorldConfig { Zelda1 = baseline }, new Graph(), new PRNG(1)).YamlData!;
        var baseExits = baseData.overworld_maps
            .Where(m => m.name != "Meta")
            .ToDictionary(m => m.map, m => (m.exit[0], m.exit[1]));

        var failures = new List<string>();
        for (int seed = 0; seed < 25; seed++)
        {
            var data = BuildWorld(seed).YamlData!;
            foreach (var map in data.overworld_maps.Where(m => m.name != "Meta"))
            {
                if (!baseExits.TryGetValue(map.map, out var want)) continue;
                if ((map.exit[0], map.exit[1]) != want)
                    failures.Add($"seed {seed} map {map.map:X2} exit changed from ({want.Item1},{want.Item2}) to ({map.exit[0]},{map.exit[1]})");
            }
        }

        Assert.AreEqual(0, failures.Count,
            "Cave/any-road exit[] must stay at the vanilla per-screen value:\n" + string.Join("\n", failures.Take(20)));
    }

    [TestMethod]
    public void FluteLandings_DropLinkOnWalkableGround()
    {
        var failures = new List<string>();

        for (int seed = 0; seed < 50; seed++)
        {
            var data = BuildWorld(seed).YamlData!;
            var sp = data.special;
            var screens = data.overworld_screens
                .Where(s => s.walkable != null)
                .ToDictionary(s => s.screen, s => s);

            for (int i = 0; i < sp.recorder_dests.Length; i++)
            {
                // recorder_dests[i] is the room LEFT of the dungeon entrance; the entrance screen
                // is one column to the right (wrapping within the 16-wide world row).
                int destMapId = (sp.recorder_dests[i] % 0x10 == 0x0f)
                    ? sp.recorder_dests[i] - 0x0f
                    : sp.recorder_dests[i] + 1;
                var map = data.overworld_maps.FirstOrDefault(m => m.map == destMapId);
                if (map == null || !screens.TryGetValue(map.screen, out var screen)) continue;
                var grid = WalkGrid(screen);
                if (grid == null) continue;

                int y = sp.recorder_y_pos[i];
                int row = (y - 0x4D) >> 4;

                // Vanilla drops Link at the level entrance (often the cave tile itself), not on
                // empty ground, and the engine steps him in/out. So when the screen has a baked
                // entrance, the landing row must match an entrance row; otherwise it must be a
                // walkable square at the drop column.
                bool ok;
                if (screen.entrances != null && screen.entrances.Count > 0)
                {
                    ok = screen.entrances.Any(e => e[1] == row);
                }
                else
                {
                    // Hidden/secret-entrance screens: Link lands on the nearest walkable ground, so
                    // require the chosen row to contain at least one walkable square.
                    ok = row >= 0 && row < OverworldWalkability.Rows &&
                         Enumerable.Range(0, OverworldWalkability.Columns).Any(c => grid[c, row]);
                }

                if (!ok)
                    failures.Add($"seed {seed} flute L{i + 1} scr {map.screen:X2} y={y:X2} row {row} doesn't match an entrance/walkable spot");
            }
        }

        Assert.AreEqual(0, failures.Count,
            "Some flute landings drop Link off walkable ground:\n" + string.Join("\n", failures.Take(20)));
    }
}
