namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class MiseryMireTest : InvertedOverworldGlitchesLogicTests
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
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Main Lobby Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Big Key Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "L4Sword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Compass Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "PegasusBoots", "Ether", "L4Sword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Bridge Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Map Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Spike Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "PegasusBoots", "Ether", "L4Sword" } },

        new object[] { "Misery Mire - Boss", false, new string[] {  } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "PegasusBoots", "Ether", "UncleSword" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "PegasusBoots", "Ether", "ProgressiveSword" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "PegasusBoots", "Ether", "MasterSword" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "PegasusBoots", "Ether", "L3Sword" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "PegasusBoots", "Ether", "L4Sword" } },
    };
}
