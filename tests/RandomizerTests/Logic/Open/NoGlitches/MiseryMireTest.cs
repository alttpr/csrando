using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class MiseryMireTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Misery Mire - Big Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Big Chest", false, new string[] { "BigKeyD6", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Chest", true, new string[] { "BigKeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },

        new object[] { "Misery Mire - Main Lobby Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Main Lobby Chest", false, new string[] { "KeyD6", "MoonPearl", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Main Lobby Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },

        new object[] { "Misery Mire - Big Key Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Big Key Chest", false, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Big Key Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },

        new object[] { "Misery Mire - Compass Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Compass Chest", false, new string[] { "KeyD6", "KeyD6", "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Compass Chest", true, new string[] { "KeyD6", "KeyD6", "KeyD6", "FireRod", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },

        new object[] { "Misery Mire - Bridge Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Bridge Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },

        new object[] { "Misery Mire - Map Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Map Chest", true, new string[] { "KeyD6", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },

        new object[] { "Misery Mire - Spike Chest", false, new string[] {  } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Spike Chest", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },

        new object[] { "Misery Mire - Boss", false, new string[] {  } },
        new object[] { "Misery Mire - Boss", false, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "UncleSword", "Hookshot" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "ProgressiveSword", "Hookshot" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "PegasusBoots" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "MasterSword", "Hookshot" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L3Sword", "Hookshot" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "PegasusBoots" } },
        new object[] { "Misery Mire - Boss", true, new string[] { "KeyD6", "KeyD6", "BigKeyD6", "Lamp", "CaneOfSomaria", "MoonPearl", "OcarinaActive", "TitansMitt", "Ether", "L4Sword", "Hookshot" } },
    };
    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void TestLogic(string location, bool expected, string[] inventory)
    {
        var randomizer = new Randomizer(new[]
        {
            new Dictionary<string, object>()
            {
                { "mode.state", "open" },
                { "logic", "NoGlitches" },
            }
        });
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
}
