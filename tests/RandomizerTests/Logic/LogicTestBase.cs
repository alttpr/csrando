namespace RandomizerTests.Logic;

using AlttpRandomizer.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class LogicTestBase
{
    protected abstract RandomizerConfig GetWorldConfig();

    protected void RunLogicTest(RandomizerConfig[] config, string location, bool expected, IEnumerable<string> inventory)
    {
        var randomizer = new Randomizer(config);
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
    
    public static string GetLogicTestDisplayNames(MethodInfo methodInfo, object[] values)
    {
        string location = (string)values[0];
        bool expected = (bool)values[1];
        string[] inventory = (string[])values[2];
        string inventory_string = inventory.Any() ? String.Join(',', inventory) : "None";

        return $"{methodInfo.Name}({location}, {expected}, {inventory_string})";
    }

    public void TestLogic(string location, bool expected, string[] inventory)
    {
        RunLogicTest(new[]
        {
            GetWorldConfig(),
        }, location, expected, inventory);
    }
}
