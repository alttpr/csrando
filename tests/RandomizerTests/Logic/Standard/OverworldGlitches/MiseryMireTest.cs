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

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Misery Mire - Big Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Main Lobby Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Big Key Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Compass Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Bridge Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Map Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Spike Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Boss", false, new string[] {  } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "PegasusBoots", "Ether", "L4Sword" } },
    };
}
