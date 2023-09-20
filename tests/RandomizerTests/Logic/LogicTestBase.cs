namespace RandomizerTests.Logic;

using AlttpRandomizer.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class LogicTestBase
{
    protected abstract WorldConfig GetWorldConfig();

    protected void RunLogicTest(WorldConfig[] config, string location, bool expected, IEnumerable<string> inventory)
    {
        var randomizer = new Randomizer(config);
        randomizer.AssumeItems(inventory.Select(i => randomizer.GetItemForWorld(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }

    public static string GetLogicTestDisplayNames(MethodInfo methodInfo, object[] values)
    {
        string location = (string)values[0];
        bool expected = (bool)values[1];
        string[] inventory = (string[])values[2];
        string inventory_string = inventory.Any()
            ? string.Join(',', inventory
                .GroupBy(s => s)
                .Select(g => g.Count() == 1
                    ? g.Single()
                    : $"{g.Count()}×{g.First()}"))
            : "None";

        return $"{methodInfo.Name}({location}, {expected}, {inventory_string})";
    }

    public virtual void TestLogic(string location, bool expected, string[] inventory)
    {
        RunLogicTest(new[]
        {
            GetWorldConfig(),
        }, location, expected, inventory);
    }
}
