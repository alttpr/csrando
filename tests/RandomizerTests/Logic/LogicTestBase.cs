namespace RandomizerTests.Logic;

using Randomizer.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class LogicTestBase
{
    protected abstract WorldConfig GetWorldConfig();

    protected void RunLogicTest(WorldConfig[] config, string location, bool expected, IEnumerable<string> inventory)
    {
        var randomizer = new Randomizer(config);
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
        RunLogicTest(
        [
            GetWorldConfig(),
        ], location, expected, inventory);
    }
}
