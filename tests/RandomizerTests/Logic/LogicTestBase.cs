namespace RandomizerTests.Logic;

using Randomizer.Games;
using Randomizer.Graph;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AlttpRandomizer = Randomizer.Games.Alttp.GameRandomizer;

public abstract class LogicTestBase
{
    protected abstract WorldConfig GetWorldConfig();

    // Tests are run in parallel in the same process, so we try to cache Randomizer instances as much as possible.
    private static readonly ConcurrentDictionary<WorldConfig[], Lazy<GameRandomizer>> _cachedRandomizers = new(new WorldConfigArrayComparer());

    protected void RunLogicTest(WorldConfig[] config, string location, bool expected, IEnumerable<string> inventory)
    {
        var randomizer = GetRandomizerForConfig(config);

        // this is a single-world test; we have exactly one player world.
        var world = randomizer.Worlds[0];
        try
        {
            _ = world.GetLocation(location);
        }
        catch (Exception)
        {
            Assert.Fail($"Location \"{location}\" doesn't exist in the graph");
        }

        var searcher = randomizer.GetSearcherForInventory(inventory.Select(world.GetItem));
        Assert.AreEqual(expected, searcher.GetVisited().Any(v => v.Name == location));
    }

    public static string GetLogicTestDisplayNames(MethodInfo methodInfo, object[] values)
    {
        string location = (string)values[0];
        bool expected = (bool)values[1];
        string[] inventory = (string[])values[2];
        string inventoryString = inventory.Any()
            ? string.Join(',', inventory
                .GroupBy(s => s)
                .Select(g => g.Count() == 1
                    ? g.Single()
                    : $"{g.Count()}×{g.First()}"))
            : "None";

        return $"{methodInfo.Name}({location}, {expected}, {inventoryString})";
    }

    public virtual void TestLogic(string location, bool expected, string[] inventory)
    {
        var singleWorldConfig = GetWorldConfig();
        // FIXME: we use the config as caching key (see GetRandomizerForConfig) but change its values as part of the randomization.
        singleWorldConfig.Alttp?.SelectRandomValues(new(seed: 42));
        RunLogicTest(
        [
            singleWorldConfig,
        ], location, expected, inventory);
    }

    protected GameRandomizer GetRandomizerForConfig(WorldConfig[] config)
    {
        return _cachedRandomizers.GetOrAdd(config, config
            => new Lazy<GameRandomizer>(() => new AlttpRandomizer(config, new(seed: 42)),
            LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    // This is crude, but easier than having a proper comparer on WorldConfig
    private sealed class WorldConfigArrayComparer : IEqualityComparer<WorldConfig[]>
    {
        public bool Equals(WorldConfig[]? x, WorldConfig[]? y)
        {
            if (x == y)
                return true;
            if (x == null || y == null)
                return false;

            var xJson = JsonSerializer.Serialize(x);
            var yJson = JsonSerializer.Serialize(y);
            return xJson.Equals(yJson);
        }

        public int GetHashCode([DisallowNull] WorldConfig[] obj)
        {
            var json = JsonSerializer.Serialize(obj);
            var hashcode = json.GetHashCode();
            return hashcode;
        }
    }
}
