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

    public static IEnumerable<object[]> TestData => [
        ["Master Sword Pedestal", false, new string[] {  }],
        ["Master Sword Pedestal", true, new string[] { "PendantOfCourage", "PendantOfWisdom", "PendantOfPower", "MoonPearl", "PegasusBoots" }],
        ["Master Sword Pedestal", true, new string[] { "PendantOfCourage", "PendantOfWisdom", "PendantOfPower", "MagicMirror", "PegasusBoots" }],

        ["Link's Uncle", false, new string[] {  }],
        ["Link's Uncle", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Secret Passage", false, new string[] {  }],
        ["Secret Passage", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["King's Tomb", false, new string[] {  }],
        ["King's Tomb", true, new string[] { "PegasusBoots", "MagicMirror", "MoonPearl" }],

        ["Floodgate Chest", false, new string[] {  }],
        ["Floodgate Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Floodgate Chest", true, new string[] { "MagicMirror", "PegasusBoots" }],

        ["Kakariko Tavern Item", false, new string[] {  }],
        ["Kakariko Tavern Item", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Kakariko Tavern Item", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Kakariko Tavern Item", true, new string[] { "DefeatAgahnim" }],

        ["Chicken House Item", false, new string[] {  }],
        ["Chicken House Item", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Aginah's Cave", false, new string[] {  }],
        ["Aginah's Cave", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Sahasrahla's Hut - Left", false, new string[] {  }],
        ["Sahasrahla's Hut - Left", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Sahasrahla's Hut - Left", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Sahasrahla's Hut - Left", true, new string[] { "DefeatAgahnim", "PegasusBoots" }],

        ["Sahasrahla's Hut - Middle", false, new string[] {  }],
        ["Sahasrahla's Hut - Middle", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Sahasrahla's Hut - Middle", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Sahasrahla's Hut - Middle", true, new string[] { "DefeatAgahnim", "PegasusBoots" }],

        ["Sahasrahla's Hut - Right", false, new string[] {  }],
        ["Sahasrahla's Hut - Right", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Sahasrahla's Hut - Right", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Sahasrahla's Hut - Right", true, new string[] { "DefeatAgahnim", "PegasusBoots" }],

        ["Kakariko Well - Top", false, new string[] {  }],
        ["Kakariko Well - Top", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Kakariko Well - Left", false, new string[] {  }],
        ["Kakariko Well - Left", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Kakariko Well - Left", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Kakariko Well - Left", true, new string[] { "DefeatAgahnim" }],

        ["Kakariko Well - Middle", false, new string[] {  }],
        ["Kakariko Well - Middle", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Kakariko Well - Middle", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Kakariko Well - Middle", true, new string[] { "DefeatAgahnim" }],

        ["Kakariko Well - Right", false, new string[] {  }],
        ["Kakariko Well - Right", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Kakariko Well - Right", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Kakariko Well - Right", true, new string[] { "DefeatAgahnim" }],

        ["Kakariko Well - Bottom", false, new string[] {  }],
        ["Kakariko Well - Bottom", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Kakariko Well - Bottom", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Kakariko Well - Bottom", true, new string[] { "DefeatAgahnim" }],

        ["Blind's Hideout - Top", false, new string[] {  }],
        ["Blind's Hideout - Top", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Blind's Hideout - Left", false, new string[] {  }],
        ["Blind's Hideout - Left", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Blind's Hideout - Left", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Blind's Hideout - Left", true, new string[] { "MagicMirror", "DefeatAgahnim" }],

        ["Blind's Hideout - Right", false, new string[] {  }],
        ["Blind's Hideout - Right", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Blind's Hideout - Right", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Blind's Hideout - Right", true, new string[] { "MagicMirror", "DefeatAgahnim" }],

        ["Blind's Hideout - Far Left", false, new string[] {  }],
        ["Blind's Hideout - Far Left", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Blind's Hideout - Far Left", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Blind's Hideout - Far Left", true, new string[] { "MagicMirror", "DefeatAgahnim" }],

        ["Blind's Hideout - Far Right", false, new string[] {  }],
        ["Blind's Hideout - Far Right", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Blind's Hideout - Far Right", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Blind's Hideout - Far Right", true, new string[] { "MagicMirror", "DefeatAgahnim" }],

        ["Bonk Rocks Item", false, new string[] {  }],
        ["Bonk Rocks Item", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Mini Moldorm Cave - Far Left", false, new string[] {  }],
        ["Mini Moldorm Cave - Far Left", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Mini Moldorm Cave - Left", false, new string[] {  }],

        ["Mini Moldorm Cave - Right", false, new string[] {  }],
        ["Mini Moldorm Cave - Right", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Mini Moldorm Cave - Far Right", false, new string[] {  }],

        ["Ice Rod Cave Item", false, new string[] {  }],
        ["Ice Rod Cave Item", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Ice Rod Cave Item", true, new string[] { "MagicMirror", "PegasusBoots", "BigRedBomb" }],
        ["Ice Rod Cave Item", true, new string[] { "MagicMirror", "DefeatAgahnim", "BigRedBomb" }],

        ["Bottle Merchant", false, new string[] {  }],
        ["Bottle Merchant", true, new string[] { "PegasusBoots", "MagicMirror" }],
        ["Bottle Merchant", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Sahasrahla's Hut - Sahasrahla", false, new string[] {  }],
        ["Sahasrahla's Hut - Sahasrahla", true, new string[] { "PendantOfCourage", "MagicMirror", "PegasusBoots" }],
        ["Sahasrahla's Hut - Sahasrahla", true, new string[] { "PendantOfCourage", "MoonPearl", "PegasusBoots" }],

        ["Magic Bat Item", false, new string[] {  }],
        ["Magic Bat Item", true, new string[] { "Powder", "PegasusBoots", "MoonPearl" }],

        ["Sick Kid Item", false, new string[] {  }],
        ["Sick Kid Item", false, new string[] { "BottleWithBee" }],
        ["Sick Kid Item", false, new string[] { "BottleWithFairy" }],
        ["Sick Kid Item", false, new string[] { "BottleWithRedPotion" }],
        ["Sick Kid Item", false, new string[] { "BottleWithGreenPotion" }],
        ["Sick Kid Item", false, new string[] { "BottleWithBluePotion" }],
        ["Sick Kid Item", false, new string[] { "Bottle" }],
        ["Sick Kid Item", false, new string[] { "BottleWithGoldBee" }],
        ["Sick Kid Item", true, new string[] { "BottleWithBee", "MagicMirror", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithBee", "MoonPearl", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithFairy", "MagicMirror", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithFairy", "MoonPearl", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithRedPotion", "MagicMirror", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithRedPotion", "MoonPearl", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithGreenPotion", "MagicMirror", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithGreenPotion", "MoonPearl", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithBluePotion", "MagicMirror", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithBluePotion", "MoonPearl", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "Bottle", "MagicMirror", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "Bottle", "MoonPearl", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithGoldBee", "MagicMirror", "PegasusBoots" }],
        ["Sick Kid Item", true, new string[] { "BottleWithGoldBee", "MoonPearl", "PegasusBoots" }],

        ["Hobo", false, new string[] {  }],
        ["Hobo", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Hobo", true, new string[] { "MoonPearl", "DefeatAgahnim" }],

        ["Bombos Tablet", false, new string[] {  }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "BookOfMudora", "PegasusBoots", "ProgressiveSword", "ProgressiveSword" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "BookOfMudora", "PegasusBoots", "L2Sword" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "BookOfMudora", "PegasusBoots", "L3Sword" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "BookOfMudora", "PegasusBoots", "L4Sword" }],
        ["Bombos Tablet", true, new string[] { "MagicMirror", "BookOfMudora", "PegasusBoots", "ProgressiveSword", "ProgressiveSword" }],
        ["Bombos Tablet", true, new string[] { "MagicMirror", "BookOfMudora", "PegasusBoots", "L2Sword" }],
        ["Bombos Tablet", true, new string[] { "MagicMirror", "BookOfMudora", "PegasusBoots", "L3Sword" }],
        ["Bombos Tablet", true, new string[] { "MagicMirror", "BookOfMudora", "PegasusBoots", "L4Sword" }],

        ["King Zora", false, new string[] {  }],
        ["King Zora", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Lost Woods Hideout Item", false, new string[] {  }],
        ["Lost Woods Hideout Item", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Lumberjack Tree", false, new string[] {  }],
        ["Lumberjack Tree", true, new string[] { "PegasusBoots", "MoonPearl", "DefeatAgahnim" }],

        ["Cave 45 Item", false, new string[] {  }],
        ["Cave 45 Item", true, new string[] { "MoonPearl",  "PegasusBoots" }],
        ["Cave 45 Item", true, new string[] { "MagicMirror",  "PegasusBoots" }],
        ["Cave 45 Item", true, new string[] { "MagicMirror",  "DefeatAgahnim" }],

        ["Graveyard Ledge", false, new string[] {  }],
        ["Graveyard Ledge", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Checkerboard Cave Item", false, new string[] {  }],
        ["Checkerboard Cave Item", true, new string[] { "ProgressiveGlove", "PegasusBoots", "MoonPearl" }],
        ["Checkerboard Cave Item", true, new string[] { "PowerGlove", "PegasusBoots", "MoonPearl" }],

        ["Mini Moldorm Cave - NPC", false, new string[] {  }],
        ["Mini Moldorm Cave - NPC", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Library", false, new string[] {  }],
        ["Library", true, new string[] { "PegasusBoots", "MoonPearl" }],
        ["Library", true, new string[] { "PegasusBoots", "MagicMirror" }],

        ["Mushroom", false, new string[] {  }],
        ["Mushroom", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Potion Shop", false, new string[] {  }],
        ["Potion Shop", true, new string[] { "Mushroom", "MoonPearl", "PegasusBoots" }],

        ["Maze Race", false, new string[] {  }],
        ["Maze Race", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Desert Ledge", false, new string[] {  }],
        ["Desert Ledge", true, new string[] { "BookOfMudora", "MagicMirror", "PegasusBoots" }],
        ["Desert Ledge", true, new string[] { "BookOfMudora", "DefeatAgahnim" }],
        ["Desert Ledge", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Lake Hylia Island", false, new string[] {  }],
        ["Lake Hylia Island", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Sunken Treasure", false, new string[] {  }],
        ["Sunken Treasure", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Sunken Treasure", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Sunken Treasure", true, new string[] { "MagicMirror", "DefeatAgahnim" }],

        ["Zora's Domain Ledge Item", false, new string[] {  }],
        ["Zora's Domain Ledge Item", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Flute Spot", false, new string[] {  }],
        ["Flute Spot", true, new string[] { "Shovel", "MoonPearl", "PegasusBoots" }],

        ["Waterfall Fairy - Left", false, new string[] {  }],
        ["Waterfall Fairy - Left", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Waterfall Fairy - Left", true, new string[] { "MoonPearl", "DefeatAgahnim" }],
        ["Waterfall Fairy - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Waterfall Fairy - Left", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Waterfall Fairy - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Waterfall Fairy - Left", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Waterfall Fairy - Right", false, new string[] {  }],
        ["Waterfall Fairy - Right", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Waterfall Fairy - Right", true, new string[] { "MoonPearl", "DefeatAgahnim" }],
        ["Waterfall Fairy - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Waterfall Fairy - Right", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Waterfall Fairy - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Waterfall Fairy - Right", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Bomb Shoppe Item", false, new string[] {  }],
        ["Bomb Shoppe Item", true, new string[] { "Crystal5", "Crystal6", "MoonPearl", "PegasusBoots" }],
        ["Bomb Shoppe Item", true, new string[] { "Crystal5", "Crystal6", "MagicMirror", "PegasusBoots" }],
        ["Bomb Shoppe Item", true, new string[] { "Crystal5", "Crystal6", "DefeatAgahnim" }],

        ["Ganon", false, new string[] {  }],
    ];
}
