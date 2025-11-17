namespace RandomizerTests.Logic.Alttp.Open.OverworldGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public sealed class IcePalaceTest : OpenOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Ice Palace - Big Key Chest", false, new string[] {  }],
        ["Ice Palace - Big Key Chest", true, new string[] { "FireRod", "CaneOfSomaria", "TitansMitt" }],

            //new object[] { "Ice Palace - Compass Chest", false, new string[] {  } },
            //new object[] { "Ice Palace - Compass Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Flippers", "FireRod" } },
            //
            //new object[] { "Ice Palace - Map Chest", false, new string[] {  } },
            //new object[] { "Ice Palace - Map Chest", true, new string[] { "ProgressiveGlove", "MoonPearl", "PegasusBoots", "Flippers", "FireRod", "Hammer", "Hookshot", "KeyD5" } },
            //
            //new object[] { "Ice Palace - Spike Room", false, new string[] {  } },
            //new object[] { "Ice Palace - Spike Room", true, new string[] { "MoonPearl", "PegasusBoots", "Flippers", "FireRod", "Hookshot", "KeyD5" } },
            //
            //new object[] { "Ice Palace - Freezor Chest", false, new string[] {  } },
            //new object[] { "Ice Palace - Freezor Chest", true, new string[] { "TitansMitt", "Bombos", "L4Sword" } },
            //
            //new object[] { "Ice Palace - Iced T Room", false, new string[] {  } },
            //new object[] { "Ice Palace - Iced T Room", true, new string[] { "TitansMitt", "Bombos", "L4Sword" } },
            //
            //new object[] { "Ice Palace - Big Chest", false, new string[] {  } },
            //new object[] { "Ice Palace - Big Chest", true, new string[] { "BigKeyD5", "TitansMitt", "Bombos", "L4Sword" } },
            //
            //new object[] { "Ice Palace - Boss", false, new string[] {  } },
            //new object[] { "Ice Palace - Boss", true, new string[] { "BigKeyD5", "TitansMitt", "Bombos", "L4Sword", "Hammer", "CaneOfSomaria", "KeyD5" } },
    ];
}
