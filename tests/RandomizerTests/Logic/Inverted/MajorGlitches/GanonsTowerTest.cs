namespace RandomizerTests.Logic.Inverted.MajorGlitches;

[TestClass]
public sealed class GanonsTowerTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Ganon's Tower - Bob's Torch", false, new string[] {  }],
        ["Ganon's Tower - Bob's Torch", true, new string[] { "PegasusBoots", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - DMs Room - Top Left", false, new string[] {  }],
        ["Ganon's Tower - DMs Room - Top Left", true, new string[] { "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - DMs Room - Top Right", false, new string[] {  }],
        ["Ganon's Tower - DMs Room - Top Right", true, new string[] { "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - DMs Room - Bottom Left", false, new string[] {  }],
        ["Ganon's Tower - DMs Room - Bottom Left", true, new string[] { "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - DMs Room - Bottom Right", false, new string[] {  }],
        ["Ganon's Tower - DMs Room - Bottom Right", true, new string[] { "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Randomizer Room - Top Left", false, new string[] {  }],
        ["Ganon's Tower - Randomizer Room - Top Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Randomizer Room - Top Right", false, new string[] {  }],
        ["Ganon's Tower - Randomizer Room - Top Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Randomizer Room - Bottom Left", false, new string[] {  }],
        ["Ganon's Tower - Randomizer Room - Bottom Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Randomizer Room - Bottom Right", false, new string[] {  }],
        ["Ganon's Tower - Randomizer Room - Bottom Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Firesnake Room", false, new string[] {  }],
        ["Ganon's Tower - Firesnake Room", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Map Chest", false, new string[] {  }],
        ["Ganon's Tower - Map Chest", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Map Chest", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "PegasusBoots", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Big Chest", false, new string[] {  }],
        ["Ganon's Tower - Big Chest", true, new string[] { "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "CaneOfSomaria", "FireRod", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Big Chest", true, new string[] { "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Hope Room - Left", false, new string[] {  }],
        ["Ganon's Tower - Hope Room - Left", true, new string[] { "Hookshot", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Hope Room - Right", false, new string[] {  }],
        ["Ganon's Tower - Hope Room - Right", true, new string[] { "Hookshot", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Bob's Chest", false, new string[] {  }],
        ["Ganon's Tower - Bob's Chest", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "CaneOfSomaria", "FireRod", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Bob's Chest", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Tile Room", false, new string[] {  }],
        ["Ganon's Tower - Tile Room", true, new string[] { "CaneOfSomaria", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Compass Room - Top Left", false, new string[] {  }],
        ["Ganon's Tower - Compass Room - Top Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "CaneOfSomaria", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Compass Room - Top Right", false, new string[] {  }],
        ["Ganon's Tower - Compass Room - Top Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "CaneOfSomaria", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Compass Room - Bottom Left", false, new string[] {  }],
        ["Ganon's Tower - Compass Room - Bottom Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "CaneOfSomaria", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Compass Room - Bottom Right", false, new string[] {  }],
        ["Ganon's Tower - Compass Room - Bottom Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "CaneOfSomaria", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Big Key Chest", false, new string[] {  }],
        ["Ganon's Tower - Big Key Chest", true, new string[] { "UncleSword", "KeyA2", "KeyA2", "KeyA2", "CaneOfSomaria", "FireRod", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Big Key Chest", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Big Key Room - Left", false, new string[] {  }],
        ["Ganon's Tower - Big Key Room - Left", true, new string[] { "UncleSword", "KeyA2", "KeyA2", "KeyA2", "CaneOfSomaria", "FireRod", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Big Key Room - Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Big Key Room - Right", false, new string[] {  }],
        ["Ganon's Tower - Big Key Room - Right", true, new string[] { "UncleSword", "KeyA2", "KeyA2", "KeyA2", "CaneOfSomaria", "FireRod", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Big Key Room - Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Hammer", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Mini Helmasaur Room - Left", false, new string[] {  }],
        ["Ganon's Tower - Mini Helmasaur Room - Left", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "Lamp", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Mini Helmasaur Room - Left", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Mini Helmasaur Room - Right", false, new string[] {  }],
        ["Ganon's Tower - Mini Helmasaur Room - Right", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "Lamp", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Mini Helmasaur Room - Right", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Pre-Moldorm Chest", false, new string[] {  }],
        ["Ganon's Tower - Pre-Moldorm Chest", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "Lamp", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Pre-Moldorm Chest", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],

        ["Ganon's Tower - Moldorm Chest", false, new string[] {  }],
        ["Ganon's Tower - Moldorm Chest", true, new string[] { "BowAndArrows", "UncleSword", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "KeyA2", "Hookshot", "Lamp", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
        ["Ganon's Tower - Moldorm Chest", true, new string[] { "BowAndArrows", "UncleSword", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "KeyA2", "Hookshot", "FireRod", "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7" }],
    ];
}
