namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class MiseryMireTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory.Concat(new[] { "MireEntryEther" }).ToArray());
    }

    public static IEnumerable<object[]> TestData => [
        ["Misery Mire - Big Chest", false, new string[] {  }],
        ["Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" }],
        ["Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" }],
        ["Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" }],
        ["Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" }],
        ["Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" }],

        ["Misery Mire - Main Lobby Chest", false, new string[] {  }],
        ["Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" }],
        ["Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" }],
        ["Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" }],
        ["Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" }],
        ["Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" }],

        ["Misery Mire - Big Key Chest", false, new string[] {  }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" }],
        ["Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" }],

        ["Misery Mire - Compass Chest", false, new string[] {  }],
        ["Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" }],
        ["Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" }],
        ["Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" }],
        ["Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" }],
        ["Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" }],

        ["Misery Mire - Bridge Chest", false, new string[] {  }],
        ["Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "UncleSword" }],
        ["Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" }],
        ["Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "MasterSword" }],
        ["Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "L3Sword" }],
        ["Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "L4Sword" }],

        ["Misery Mire - Map Chest", false, new string[] {  }],
        ["Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" }],
        ["Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" }],
        ["Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" }],
        ["Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" }],
        ["Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" }],

        ["Misery Mire - Spike Chest", false, new string[] {  }],
        ["Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "UncleSword" }],
        ["Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" }],
        ["Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "MasterSword" }],
        ["Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "L3Sword" }],
        ["Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "L4Sword" }],

        ["Misery Mire - Boss", false, new string[] {  }],
        ["Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" }],
        ["Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" }],
        ["Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" }],
        ["Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" }],
        ["Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" }],
    ];
}
