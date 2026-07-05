namespace RandomizerTests.Games.Zelda1;

using System.Collections.Generic;
using System.Linq;
using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;

/// <summary>
/// The overworld level-info table is shared between first and second quest, so screens carrying a
/// SECOND-QUEST-only hidden entrance (level-info F bit 0x80, modelled as secret[1] == 1 — e.g. the
/// burn-bush Level 8 entrance on screen 0x67) still appear in the data. They must NOT be wired into
/// the first-quest logic graph: in first quest the engine never reveals them, so a logical edge into
/// a dungeon/cave through one is a phantom entrance that can certify an actually-unwinnable seed.
/// </summary>
[TestClass]
public sealed class SecondQuestEntranceLeakTest
{
    private static World BuildWorld(EntranceShuffleOption shuffle, int seed)
    {
        var config = new Config { EntranceShuffle = shuffle };
        return new World(0, new WorldConfig { Zelda1 = config }, new Graph(), new PRNG(seed));
    }

    private static bool IsDungeonOrCaveEntrance(string name)
        => name.EndsWith(" - Entrance") && (name.StartsWith("Level ") || name.StartsWith("Cave "));

    // A second-quest entrance leak shows up as a graph edge from a node owned by a secret[1] == 1
    // overworld map into a "Level N - Entrance" / "Cave XX - Entrance" node.
    private static List<string> FindLeaks(World world)
    {
        var data = world.YamlData!;
        // Second-quest-ONLY entrances are secret[1] == 1 with secret[0] == 0. A [1,1] screen is
        // reachable in first quest too and must stay wired, so it is deliberately not flagged here.
        var secondQuestPrefixes = data.overworld_maps
            .Where(m => m.name != "Meta" && m.secret[1] == 1 && m.secret[0] == 0)
            .Select(m => $"{m.area} - {m.name} - ")
            .ToList();

        var leaks = new List<string>();
        foreach (var vertex in world.Graph.GetVertices())
        {
            var prefix = secondQuestPrefixes.FirstOrDefault(p => vertex.Name.StartsWith(p));
            if (prefix == null)
                continue;

            foreach (var edge in vertex.Edges)
            {
                if (IsDungeonOrCaveEntrance(edge.To.Name))
                    leaks.Add($"{vertex.Name} -> {edge.To.Name}");
            }
        }

        return leaks;
    }

    [TestMethod]
    public void SecondQuestEntrances_DoNotWireIntoDungeons_Unshuffled()
    {
        var leaks = FindLeaks(BuildWorld(EntranceShuffleOption.None, 1));
        Assert.AreEqual(0, leaks.Count,
            "Second-quest-only entrances leaked into the first-quest graph:\n" + string.Join("\n", leaks));
    }

    [TestMethod]
    [TestCategory(TestCategories.Slow)]
    public void SecondQuestEntrances_DoNotWireIntoDungeons_OverworldShuffle()
    {
        var failures = new List<string>();
        for (int seed = 0; seed < 50; seed++)
        {
            var leaks = FindLeaks(BuildWorld(EntranceShuffleOption.Overworld, seed));
            if (leaks.Count > 0)
                failures.Add($"seed {seed}: " + string.Join(", ", leaks));
        }

        Assert.AreEqual(0, failures.Count,
            "Second-quest-only entrances leaked into the shuffled graph:\n" + string.Join("\n", failures.Take(20)));
    }

    [TestMethod]
    public void Level8_BurnBushSecondQuestScreen_IsNotAnEntrance()
    {
        // Concrete regression: map 0x6C / screen 0x67 is the second-quest burn-bush Level 8 entrance
        // (secret = [0, 1]). It must not appear as a logical Level 8 entrance in first quest.
        var world = BuildWorld(EntranceShuffleOption.None, 1);
        var map6C = world.YamlData!.overworld_maps.Single(m => m.map == 0x6C);
        Assert.AreEqual(1, map6C.secret[1], "Expected map 0x6C to be flagged second-quest (secret[1] == 1)");
        Assert.AreEqual(0, map6C.secret[0], "Expected map 0x6C to be second-quest ONLY (secret[0] == 0)");

        var caveNodeName = $"{map6C.area} - {map6C.name} - Tree cave";
        bool wired = world.Graph.GetVertices()
            .Where(v => v.Name == caveNodeName)
            .SelectMany(v => v.Edges)
            .Any(e => IsDungeonOrCaveEntrance(e.To.Name));

        Assert.IsFalse(wired, $"`{caveNodeName}` must not be wired to a dungeon/cave entrance in first quest");
    }
}
