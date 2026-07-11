namespace RandomizerTests.Games.Zelda1;

using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;
using Graph = Randomizer.Graph.Graph;

[TestClass]
public sealed class MapPlacementTest
{
    // Run the full randomizer (assumed fill) with dungeon shuffle and the given map-placement mode.
    private static World RandomizeWorld(MapPlacementOption mapPlacement, int seed)
    {
        var z1Config = new Config
        {
            DungeonShuffle = true,
            DungeonStyle = DungeonStyleOption.Progressive,
            EnemyPlacement = EnemyPlacementOption.Progressive,
            MapPlacement = mapPlacement,
            Triforces = "8",
        };
        z1Config.SelectRandomValues(new PRNG(seed));
        var randomizer = new Randomizer.Games.Zelda1.GameRandomizer(
            [new WorldConfig { Zelda1 = z1Config }], new PRNG(seed));
        randomizer.Randomize();
        return (World)randomizer.Worlds[0];
    }

    private static bool IsDungeonMapSet(ItemSetName set, int level) =>
        set.Name == $"z1d{level}m";

    [DataTestMethod]
    [TestCategory(TestCategories.Slow)]
    [DataRow(MapPlacementOption.Early, 1)]
    [DataRow(MapPlacementOption.Early, 42)]
    [DataRow(MapPlacementOption.Early, 99)]
    [DataRow(MapPlacementOption.Early, 256)]
    [DataRow(MapPlacementOption.Closest, 7)]
    [DataRow(MapPlacementOption.Closest, 314)]
    [DataRow(MapPlacementOption.Closest, 1000)]
    [DataRow(MapPlacementOption.Closest, 5)]
    public void Randomize_MapPlacementOn_EveryMapLandsInEntranceSet(MapPlacementOption placement, int seed)
    {
        // End-to-end: the full assumed fill must complete, and each dungeon's Map must end up in a
        // location belonging to that dungeon's tighter z1d{level}m set (the entrance-adjacent room).
        var world = RandomizeWorld(placement, seed);
        var mapItem = world.GetItem("Map");

        var mapLocations = world.GetLocationsOfType(VertexType.Item)
            .Where(v => v.Item == mapItem)
            .ToList();

        Assert.AreEqual(9, mapLocations.Count, $"Seed {seed} ({placement}): expected 9 placed Maps (one per dungeon).");

        foreach (var loc in mapLocations)
        {
            bool inMapSet = Enumerable.Range(1, 9).Any(level => loc.ItemSet.Any(s => IsDungeonMapSet(s, level)));
            Assert.IsTrue(inMapSet,
                $"Seed {seed} ({placement}): Map placed in '{loc.Name}', whose item sets " +
                $"[{string.Join(", ", loc.ItemSet.Select(s => s.Name))}] include no z1d{{level}}m entrance set.");
        }
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Slow)]
    [DataRow(1)]
    [DataRow(42)]
    public void Randomize_MapPlacementOff_NoEntranceSetExists(int seed)
    {
        // With the option off, no z1d{level}m set should be attached to any location, and the fill
        // still completes normally (regression guard for the default path).
        var world = RandomizeWorld(MapPlacementOption.Off, seed);

        bool anyMapSet = world.GetLocationsOfType(VertexType.Item)
            .Any(v => v.ItemSet.Any(s => Enumerable.Range(1, 9).Any(level => IsDungeonMapSet(s, level))));

        Assert.IsFalse(anyMapSet,
            $"Seed {seed}: MapPlacement=Off must not create any z1d{{level}}m entrance set.");
    }
}
