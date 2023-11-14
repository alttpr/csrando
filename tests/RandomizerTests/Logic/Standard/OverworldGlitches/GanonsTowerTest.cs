namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class GanonsTowerTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Ganon's Tower - Bob's Torch", false, new string[] {  }],
        ["Ganon's Tower - Bob's Torch", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Ganon's Tower - DMs Room - Top Left", false, new string[] {  }],
        ["Ganon's Tower - DMs Room - Top Left", true, new string[] { "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - DMs Room - Top Right", false, new string[] {  }],
        ["Ganon's Tower - DMs Room - Top Right", true, new string[] { "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - DMs Room - Bottom Left", false, new string[] {  }],
        ["Ganon's Tower - DMs Room - Bottom Left", true, new string[] { "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - DMs Room - Bottom Right", false, new string[] {  }],
        ["Ganon's Tower - DMs Room - Bottom Right", true, new string[] { "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Randomizer Room - Top Left", false, new string[] {  }],
        ["Ganon's Tower - Randomizer Room - Top Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Randomizer Room - Top Right", false, new string[] {  }],
        ["Ganon's Tower - Randomizer Room - Top Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Randomizer Room - Bottom Left", false, new string[] {  }],
        ["Ganon's Tower - Randomizer Room - Bottom Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Randomizer Room - Bottom Right", false, new string[] {  }],
        ["Ganon's Tower - Randomizer Room - Bottom Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Firesnake Room", false, new string[] {  }],
        ["Ganon's Tower - Firesnake Room", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Map Chest", false, new string[] {  }],
        ["Ganon's Tower - Map Chest", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Big Chest", false, new string[] {  }],
        ["Ganon's Tower - Big Chest", true, new string[] { "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "CaneOfSomaria", "FireRod", "PegasusBoots" }],
        ["Ganon's Tower - Big Chest", true, new string[] { "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Hope Room - Left", false, new string[] {  }],
        ["Ganon's Tower - Hope Room - Left", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Ganon's Tower - Hope Room - Right", false, new string[] {  }],
        ["Ganon's Tower - Hope Room - Right", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Ganon's Tower - Bob's Chest", false, new string[] {  }],
        ["Ganon's Tower - Bob's Chest", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "CaneOfSomaria", "FireRod", "PegasusBoots" }],
        ["Ganon's Tower - Bob's Chest", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Tile Room", false, new string[] {  }],
        ["Ganon's Tower - Tile Room", true, new string[] { "MoonPearl", "TitansMitt", "CaneOfSomaria", "PegasusBoots" }],

        ["Ganon's Tower - Compass Room - Top Left", false, new string[] {  }],
        ["Ganon's Tower - Compass Room - Top Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "MoonPearl", "CaneOfSomaria", "PegasusBoots" }],

        ["Ganon's Tower - Compass Room - Top Right", false, new string[] {  }],
        ["Ganon's Tower - Compass Room - Top Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "MoonPearl", "CaneOfSomaria", "PegasusBoots" }],

        ["Ganon's Tower - Compass Room - Bottom Left", false, new string[] {  }],
        ["Ganon's Tower - Compass Room - Bottom Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "MoonPearl", "CaneOfSomaria", "PegasusBoots" }],

        ["Ganon's Tower - Compass Room - Bottom Right", false, new string[] {  }],
        ["Ganon's Tower - Compass Room - Bottom Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "KeyA2", "FireRod", "MoonPearl", "CaneOfSomaria", "PegasusBoots" }],

        ["Ganon's Tower - Big Key Chest", false, new string[] {  }],
        ["Ganon's Tower - Big Key Chest", true, new string[] { "UncleSword", "KeyA2", "KeyA2", "KeyA2", "MoonPearl",  "CaneOfSomaria", "FireRod", "PegasusBoots" }],
        ["Ganon's Tower - Big Key Chest", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Big Key Room - Left", false, new string[] {  }],
        ["Ganon's Tower - Big Key Room - Left", true, new string[] { "UncleSword", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "CaneOfSomaria", "FireRod", "PegasusBoots" }],
        ["Ganon's Tower - Big Key Room - Left", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Big Key Room - Right", false, new string[] {  }],
        ["Ganon's Tower - Big Key Room - Right", true, new string[] { "UncleSword", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "CaneOfSomaria", "FireRod", "PegasusBoots" }],
        ["Ganon's Tower - Big Key Room - Right", true, new string[] { "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "Hammer", "PegasusBoots" }],

        ["Ganon's Tower - Mini Helmasaur Room - Left", false, new string[] {  }],
        ["Ganon's Tower - Mini Helmasaur Room - Left", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Lamp", "PegasusBoots" }],
        ["Ganon's Tower - Mini Helmasaur Room - Left", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "FireRod", "PegasusBoots" }],

        ["Ganon's Tower - Mini Helmasaur Room - Right", false, new string[] {  }],
        ["Ganon's Tower - Mini Helmasaur Room - Right", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Lamp", "PegasusBoots" }],
        ["Ganon's Tower - Mini Helmasaur Room - Right", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "FireRod", "PegasusBoots" }],

        ["Ganon's Tower - Pre-Moldorm Chest", false, new string[] {  }],
        ["Ganon's Tower - Pre-Moldorm Chest", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Lamp", "PegasusBoots" }],
        ["Ganon's Tower - Pre-Moldorm Chest", true, new string[] { "BowAndArrows", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "FireRod", "PegasusBoots" }],

        ["Ganon's Tower - Moldorm Chest", false, new string[] {  }],
        ["Ganon's Tower - Moldorm Chest", true, new string[] { "BowAndArrows", "UncleSword", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Lamp", "Hookshot", "PegasusBoots" }],
        ["Ganon's Tower - Moldorm Chest", true, new string[] { "BowAndArrows", "UncleSword", "BigKeyA2", "KeyA2", "KeyA2", "KeyA2", "KeyA2", "MoonPearl", "Hookshot", "FireRod", "PegasusBoots" }],
    ];
}
