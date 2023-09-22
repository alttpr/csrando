namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class LightWorldTest : InvertedOverworldGlitchesLogicTests
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
        new object[] { "Master Sword Pedestal", true, new string[] { "PendantOfCourage", "PendantOfWisdom", "PendantOfPower", "MoonPearl", "PegasusBoots" } },
        new object[] { "Master Sword Pedestal", true, new string[] { "PendantOfCourage", "PendantOfWisdom", "PendantOfPower", "MagicMirror", "PegasusBoots" } },

        new object[] { "Link's Uncle", false, new string[] {  } },
        new object[] { "Link's Uncle", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Secret Passage", false, new string[] {  } },
        new object[] { "Secret Passage", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "King's Tomb", false, new string[] {  } },
        new object[] { "King's Tomb", true, new string[] { "PegasusBoots", "MagicMirror", "MoonPearl" } },

        new object[] { "Floodgate Chest", false, new string[] {  } },
        new object[] { "Floodgate Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Floodgate Chest", true, new string[] { "MagicMirror", "PegasusBoots" } },

        new object[] { "Kakariko Tavern", false, new string[] {  } },
        new object[] { "Kakariko Tavern", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Kakariko Tavern", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Kakariko Tavern", true, new string[] { "DefeatAgahnim" } },

        new object[] { "Chicken House", false, new string[] {  } },
        new object[] { "Chicken House", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Aginah's Cave", false, new string[] {  } },
        new object[] { "Aginah's Cave", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Sahasrahla's Hut - Left", false, new string[] {  } },
        new object[] { "Sahasrahla's Hut - Left", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Sahasrahla's Hut - Left", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Sahasrahla's Hut - Left", true, new string[] { "DefeatAgahnim", "PegasusBoots" } },

        new object[] { "Sahasrahla's Hut - Middle", false, new string[] {  } },
        new object[] { "Sahasrahla's Hut - Middle", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Sahasrahla's Hut - Middle", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Sahasrahla's Hut - Middle", true, new string[] { "DefeatAgahnim", "PegasusBoots" } },

        new object[] { "Sahasrahla's Hut - Right", false, new string[] {  } },
        new object[] { "Sahasrahla's Hut - Right", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Sahasrahla's Hut - Right", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Sahasrahla's Hut - Right", true, new string[] { "DefeatAgahnim", "PegasusBoots" } },

        new object[] { "Kakariko Well - Top", false, new string[] {  } },
        new object[] { "Kakariko Well - Top", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Kakariko Well - Left", false, new string[] {  } },
        new object[] { "Kakariko Well - Left", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Kakariko Well - Left", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Kakariko Well - Left", true, new string[] { "DefeatAgahnim" } },

        new object[] { "Kakariko Well - Middle", false, new string[] {  } },
        new object[] { "Kakariko Well - Middle", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Kakariko Well - Middle", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Kakariko Well - Middle", true, new string[] { "DefeatAgahnim" } },

        new object[] { "Kakariko Well - Right", false, new string[] {  } },
        new object[] { "Kakariko Well - Right", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Kakariko Well - Right", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Kakariko Well - Right", true, new string[] { "DefeatAgahnim" } },

        new object[] { "Kakariko Well - Bottom", false, new string[] {  } },
        new object[] { "Kakariko Well - Bottom", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Kakariko Well - Bottom", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Kakariko Well - Bottom", true, new string[] { "DefeatAgahnim" } },

        new object[] { "Blind's Hideout - Top", false, new string[] {  } },
        new object[] { "Blind's Hideout - Top", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Blind's Hideout - Left", false, new string[] {  } },
        new object[] { "Blind's Hideout - Left", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Blind's Hideout - Left", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Blind's Hideout - Left", true, new string[] { "MagicMirror", "DefeatAgahnim" } },

        new object[] { "Blind's Hideout - Right", false, new string[] {  } },
        new object[] { "Blind's Hideout - Right", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Blind's Hideout - Right", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Blind's Hideout - Right", true, new string[] { "MagicMirror", "DefeatAgahnim" } },

        new object[] { "Blind's Hideout - Far Left", false, new string[] {  } },
        new object[] { "Blind's Hideout - Far Left", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Blind's Hideout - Far Left", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Blind's Hideout - Far Left", true, new string[] { "MagicMirror", "DefeatAgahnim" } },

        new object[] { "Blind's Hideout - Far Right", false, new string[] {  } },
        new object[] { "Blind's Hideout - Far Right", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Blind's Hideout - Far Right", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Blind's Hideout - Far Right", true, new string[] { "MagicMirror", "DefeatAgahnim" } },

        new object[] { "Pegasus Rocks", false, new string[] {  } },
        new object[] { "Pegasus Rocks", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Mini Moldorm Cave - Far Left", false, new string[] {  } },
        new object[] { "Mini Moldorm Cave - Far Left", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Mini Moldorm Cave - Left", false, new string[] {  } },

        new object[] { "Mini Moldorm Cave - Right", false, new string[] {  } },
        new object[] { "Mini Moldorm Cave - Right", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Mini Moldorm Cave - Far Right", false, new string[] {  } },

        new object[] { "Ice Rod Cave", false, new string[] {  } },
        new object[] { "Ice Rod Cave", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Ice Rod Cave", true, new string[] { "MagicMirror", "PegasusBoots", "BigRedBomb" } },
        new object[] { "Ice Rod Cave", true, new string[] { "MagicMirror", "DefeatAgahnim", "BigRedBomb" } },

        new object[] { "Bottle Merchant", false, new string[] {  } },
        new object[] { "Bottle Merchant", true, new string[] { "PegasusBoots", "MagicMirror" } },
        new object[] { "Bottle Merchant", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Sahasrahla's Hut - Sahasrahla", false, new string[] {  } },
        new object[] { "Sahasrahla's Hut - Sahasrahla", true, new string[] { "PendantOfCourage", "MagicMirror", "PegasusBoots" } },
        new object[] { "Sahasrahla's Hut - Sahasrahla", true, new string[] { "PendantOfCourage", "MoonPearl", "PegasusBoots" } },

        new object[] { "Magic Bat", false, new string[] {  } },
        new object[] { "Magic Bat", true, new string[] { "Powder", "PegasusBoots", "MoonPearl" } },

        new object[] { "Sick Kid", false, new string[] {  } },
        new object[] { "Sick Kid", false, new string[] { "BottleWithBee" } },
        new object[] { "Sick Kid", false, new string[] { "BottleWithFairy" } },
        new object[] { "Sick Kid", false, new string[] { "BottleWithRedPotion" } },
        new object[] { "Sick Kid", false, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Sick Kid", false, new string[] { "BottleWithBluePotion" } },
        new object[] { "Sick Kid", false, new string[] { "Bottle" } },
        new object[] { "Sick Kid", false, new string[] { "BottleWithGoldBee" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithBee", "MagicMirror", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithBee", "MoonPearl", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithFairy", "MagicMirror", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithFairy", "MoonPearl", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithRedPotion", "MagicMirror", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithRedPotion", "MoonPearl", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithGreenPotion", "MagicMirror", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithGreenPotion", "MoonPearl", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithBluePotion", "MagicMirror", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithBluePotion", "MoonPearl", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "Bottle", "MagicMirror", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "Bottle", "MoonPearl", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithGoldBee", "MagicMirror", "PegasusBoots" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithGoldBee", "MoonPearl", "PegasusBoots" } },

        new object[] { "Hobo", false, new string[] {  } },
        new object[] { "Hobo", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Hobo", true, new string[] { "MoonPearl", "DefeatAgahnim" } },

        new object[] { "Bombos Tablet", false, new string[] {  } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "BookOfMudora", "PegasusBoots", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "BookOfMudora", "PegasusBoots", "L2Sword" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "BookOfMudora", "PegasusBoots", "L3Sword" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "BookOfMudora", "PegasusBoots", "L4Sword" } },
        new object[] { "Bombos Tablet", true, new string[] { "MagicMirror", "BookOfMudora", "PegasusBoots", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Bombos Tablet", true, new string[] { "MagicMirror", "BookOfMudora", "PegasusBoots", "L2Sword" } },
        new object[] { "Bombos Tablet", true, new string[] { "MagicMirror", "BookOfMudora", "PegasusBoots", "L3Sword" } },
        new object[] { "Bombos Tablet", true, new string[] { "MagicMirror", "BookOfMudora", "PegasusBoots", "L4Sword" } },

        new object[] { "King Zora", false, new string[] {  } },
        new object[] { "King Zora", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Lost Woods Hideout", false, new string[] {  } },
        new object[] { "Lost Woods Hideout", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Lumberjack Tree", false, new string[] {  } },
        new object[] { "Lumberjack Tree", true, new string[] { "PegasusBoots", "MoonPearl", "DefeatAgahnim" } },

        new object[] { "Cave 45", false, new string[] {  } },
        new object[] { "Cave 45", true, new string[] { "MoonPearl",  "PegasusBoots" } },
        new object[] { "Cave 45", true, new string[] { "MagicMirror",  "PegasusBoots" } },
        new object[] { "Cave 45", true, new string[] { "MagicMirror",  "DefeatAgahnim" } },

        new object[] { "Graveyard Ledge", false, new string[] {  } },
        new object[] { "Graveyard Ledge", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Checkerboard Cave", false, new string[] {  } },
        new object[] { "Checkerboard Cave", true, new string[] { "ProgressiveGlove", "PegasusBoots", "MoonPearl" } },
        new object[] { "Checkerboard Cave", true, new string[] { "PowerGlove", "PegasusBoots", "MoonPearl" } },

        new object[] { "Mini Moldorm Cave - NPC", false, new string[] {  } },
        new object[] { "Mini Moldorm Cave - NPC", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Library", false, new string[] {  } },
        new object[] { "Library", true, new string[] { "PegasusBoots", "MoonPearl" } },
        new object[] { "Library", true, new string[] { "PegasusBoots", "MagicMirror" } },

        new object[] { "Mushroom", false, new string[] {  } },
        new object[] { "Mushroom", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Potion Shop", false, new string[] {  } },
        new object[] { "Potion Shop", true, new string[] { "Mushroom", "MoonPearl", "PegasusBoots" } },

        new object[] { "Maze Race", false, new string[] {  } },
        new object[] { "Maze Race", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Desert Ledge", false, new string[] {  } },
        new object[] { "Desert Ledge", true, new string[] { "BookOfMudora", "MagicMirror", "PegasusBoots" } },
        new object[] { "Desert Ledge", true, new string[] { "BookOfMudora", "DefeatAgahnim" } },
        new object[] { "Desert Ledge", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Lake Hylia Island", false, new string[] {  } },
        new object[] { "Lake Hylia Island", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Sunken Treasure", false, new string[] {  } },
        new object[] { "Sunken Treasure", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Sunken Treasure", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Sunken Treasure", true, new string[] { "MagicMirror", "DefeatAgahnim" } },

        new object[] { "Zora's Domain Ledge Item", false, new string[] {  } },
        new object[] { "Zora's Domain Ledge Item", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Flute Spot", false, new string[] {  } },
        new object[] { "Flute Spot", true, new string[] { "Shovel", "MoonPearl", "PegasusBoots" } },

        new object[] { "Waterfall Fairy - Left", false, new string[] {  } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "MoonPearl", "DefeatAgahnim" } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Waterfall Fairy - Right", false, new string[] {  } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "MoonPearl", "DefeatAgahnim" } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Bomb Shoppe Item", false, new string[] {  } },
        new object[] { "Bomb Shoppe Item", true, new string[] { "Crystal5", "Crystal6", "MoonPearl", "PegasusBoots" } },
        new object[] { "Bomb Shoppe Item", true, new string[] { "Crystal5", "Crystal6", "MagicMirror", "PegasusBoots" } },
        new object[] { "Bomb Shoppe Item", true, new string[] { "Crystal5", "Crystal6", "DefeatAgahnim" } },

        new object[] { "Ganon", false, new string[] {  } },
    };
}
