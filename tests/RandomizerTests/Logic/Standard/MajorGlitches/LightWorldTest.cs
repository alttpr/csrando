namespace RandomizerTests.Logic.Standard.MajorGlitches;

[TestClass]
public sealed class LightWorldTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Master Sword Pedestal", false, new string[] {  } },
        new object[] { "Master Sword Pedestal", true, new string[] { "PendantOfCourage", "PendantOfWisdom", "PendantOfPower" } },

        new object[] { "Link's Uncle", true, new string[] {  } },

        new object[] { "Secret Passage", true, new string[] { "UncleSword" } },

        new object[] { "King's Tomb", false, new string[] {  } },
        new object[] { "King's Tomb", true, new string[] { "PegasusBoots" } },

        new object[] { "Floodgate Chest", true, new string[] {  } },

        new object[] { "Link's House", true, new string[] {  } },

        new object[] { "Kakariko Tavern Item", true, new string[] {  } },

        new object[] { "Chicken House Item", true, new string[] {  } },

        new object[] { "Aginah's Cave", true, new string[] {  } },

        new object[] { "Sahasrahla's Hut - Left", true, new string[] {  } },

        new object[] { "Sahasrahla's Hut - Middle", true, new string[] {  } },

        new object[] { "Sahasrahla's Hut - Right", true, new string[] {  } },

        new object[] { "Kakariko Well - Top", true, new string[] {  } },

        new object[] { "Kakariko Well - Left", true, new string[] {  } },

        new object[] { "Kakariko Well - Middle", true, new string[] {  } },

        new object[] { "Kakariko Well - Right", true, new string[] {  } },

        new object[] { "Kakariko Well - Bottom", true, new string[] {  } },

        new object[] { "Blind's Hideout - Top", true, new string[] {  } },

        new object[] { "Blind's Hideout - Left", true, new string[] {  } },

        new object[] { "Blind's Hideout - Right", true, new string[] {  } },

        new object[] { "Blind's Hideout - Far Left", true, new string[] {  } },

        new object[] { "Blind's Hideout - Far Right", true, new string[] {  } },

        new object[] { "Bonk Rocks Item", false, new string[] {  } },
        new object[] { "Bonk Rocks Item", true, new string[] { "PegasusBoots" } },

        new object[] { "Mini Moldorm Cave - Far Left", true, new string[] {  } },

        new object[] { "Mini Moldorm Cave - Left", true, new string[] {  } },

        new object[] { "Mini Moldorm Cave - Right", true, new string[] {  } },

        new object[] { "Mini Moldorm Cave - Far Right", true, new string[] {  } },

        new object[] { "Ice Rod Cave Item", true, new string[] {  } },

        new object[] { "Bottle Merchant", true, new string[] {  } },

        new object[] { "Sahasrahla's Hut - Sahasrahla", false, new string[] {  } },
        new object[] { "Sahasrahla's Hut - Sahasrahla", true, new string[] { "PendantOfCourage" } },

        new object[] { "Magic Bat Item", false, new string[] {  } },
        new object[] { "Magic Bat Item", true, new string[] { "Powder" } },

        new object[] { "Sick Kid Item", false, new string[] {  } },
        new object[] { "Sick Kid Item", true, new string[] { "BottleWithBee" } },
        new object[] { "Sick Kid Item", true, new string[] { "BottleWithFairy" } },
        new object[] { "Sick Kid Item", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Sick Kid Item", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Sick Kid Item", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Sick Kid Item", true, new string[] { "Bottle" } },
        new object[] { "Sick Kid Item", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Hobo", true, new string[] {  } },

        new object[] { "Bombos Tablet", false, new string[] {  } },
        new object[] { "Bombos Tablet", true, new string[] { "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Bombos Tablet", true, new string[] { "BookOfMudora", "L2Sword" } },
        new object[] { "Bombos Tablet", true, new string[] { "BookOfMudora", "L3Sword" } },
        new object[] { "Bombos Tablet", true, new string[] { "BookOfMudora", "L4Sword" } },

        new object[] { "King Zora", true, new string[] {  } },

        new object[] { "Lost Woods Hideout Item", true, new string[] {  } },

        new object[] { "Lumberjack Tree", false, new string[] {  } },
        new object[] { "Lumberjack Tree", true, new string[] { "PegasusBoots", "DefeatAgahnim" } },

        new object[] { "Cave 45 Item", true, new string[] {  } },

        new object[] { "Graveyard Ledge", true, new string[] {  } },

        new object[] { "Checkerboard Cave Item", false, new string[] {  } },
        new object[] { "Checkerboard Cave Item", true, new string[] { "ProgressiveGlove" } },
        new object[] { "Checkerboard Cave Item", true, new string[] { "PowerGlove" } },
        new object[] { "Checkerboard Cave Item", true, new string[] { "TitansMitt" } },

        new object[] { "Mini Moldorm Cave - NPC", true, new string[] {  } },

        new object[] { "Library", false, new string[] {  } },
        new object[] { "Library", true, new string[] { "PegasusBoots" } },

        new object[] { "Mushroom", true, new string[] {  } },

        new object[] { "Potion Shop", false, new string[] {  } },
        new object[] { "Potion Shop", true, new string[] { "Mushroom" } },

        new object[] { "Maze Race", true, new string[] {  } },

        new object[] { "Desert Ledge", true, new string[] {  } },

        new object[] { "Lake Hylia Island", true, new string[] {  } },

        new object[] { "Sunken Treasure", true, new string[] {  } },

        new object[] { "Zora's Domain Ledge Item", false, new string[] {  } },
        new object[] { "Zora's Domain Ledge Item", true, new string[] { "Flippers" } },
        new object[] { "Zora's Domain Ledge Item", true, new string[] { "PegasusBoots" } },

        new object[] { "Flute Spot", false, new string[] {  } },
        new object[] { "Flute Spot", true, new string[] { "Shovel" } },

        new object[] { "Waterfall Fairy - Left", false, new string[] {  } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "Flippers" } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "MoonPearl" } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "PegasusBoots" } },

        new object[] { "Waterfall Fairy - Right", false, new string[] {  } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "Flippers" } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "MoonPearl" } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "PegasusBoots" } },
    };
}
