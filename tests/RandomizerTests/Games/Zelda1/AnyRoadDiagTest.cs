namespace RandomizerTests.Games.Zelda1;

using System;
using System.Linq;
using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;
using Randomizer.RomModifications;

[TestClass]
public sealed class AnyRoadDiagTest
{
    private static (int[] arrayScreens, int[] caveScreens) RunFill(int seed, bool dungeon)
    {
        var config = new Config
        {
            EntranceShuffle = EntranceShuffleOption.Overworld,
            DungeonShuffle = dungeon,
        };
        var world = new World(0, new WorldConfig { Zelda1 = config }, new Randomizer.Graph.Graph(), new PRNG(seed));
        var data = world.YamlData!;

        var arrayScreens = data.levels[0].cellar_room_id_array.Take(4).OrderBy(x => x).ToArray();
        var caveScreens = data.overworld_maps
            .Where(m => m.name != "Meta" && m.cave == 0x14)
            .Select(m => m.map).OrderBy(x => x).ToArray();
        return (arrayScreens, caveScreens);
    }

    [TestMethod]
    public void CompareEntranceOnly_vs_EntrancePlusDungeon()
    {
        int reported = 0;
        for (int seed = 0; seed < 400 && reported < 10; seed++)
        {
            (int[] arr, int[] cave) e, d;
            try { e = RunFill(seed, dungeon: false); }
            catch (Exception ex) { Console.WriteLine($"seed {seed} entrance-only threw {ex.GetType().Name}"); continue; }
            try { d = RunFill(seed, dungeon: true); }
            catch (Exception ex) { Console.WriteLine($"seed {seed} entrance+dungeon threw {ex.GetType().Name}"); continue; }

            bool eBad = e.cave.Length != 4 || !e.arr.SequenceEqual(e.cave);
            bool dBad = d.cave.Length != 4 || !d.arr.SequenceEqual(d.cave);

            if (eBad || dBad)
            {
                reported++;
                Console.WriteLine($"SEED {seed}:");
                Console.WriteLine($"  entrance-only : arr=[{Hex(e.arr)}] cave14=[{Hex(e.cave)}] {(eBad ? "BAD" : "ok")}");
                Console.WriteLine($"  ent+dungeon   : arr=[{Hex(d.arr)}] cave14=[{Hex(d.cave)}] {(dBad ? "BAD" : "ok")}");
            }
        }
        if (reported == 0)
            Console.WriteLine("No mismatch in 400 seeds (either mode).");
    }

    private static string Hex(int[] v) => string.Join(",", v.Select(x => x.ToString("X2")));
}
