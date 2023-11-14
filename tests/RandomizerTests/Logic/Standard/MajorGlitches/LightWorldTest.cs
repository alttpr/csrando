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

    public static IEnumerable<object[]> TestData => [
        ["Master Sword Pedestal", false, new string[] {  }],
        ["Master Sword Pedestal", true, new string[] { "PendantOfCourage", "PendantOfWisdom", "PendantOfPower" }],

        ["Link's Uncle", true, new string[] {  }],

        ["Secret Passage", true, new string[] { "UncleSword" }],

        ["King's Tomb", false, new string[] {  }],
        ["King's Tomb", true, new string[] { "PegasusBoots" }],

        ["Floodgate Chest", true, new string[] {  }],

        ["Link's House", true, new string[] {  }],

        ["Kakariko Tavern Item", true, new string[] {  }],

        ["Chicken House Item", true, new string[] {  }],

        ["Aginah's Cave", true, new string[] {  }],

        ["Sahasrahla's Hut - Left", true, new string[] {  }],

        ["Sahasrahla's Hut - Middle", true, new string[] {  }],

        ["Sahasrahla's Hut - Right", true, new string[] {  }],

        ["Kakariko Well - Top", true, new string[] {  }],

        ["Kakariko Well - Left", true, new string[] {  }],

        ["Kakariko Well - Middle", true, new string[] {  }],

        ["Kakariko Well - Right", true, new string[] {  }],

        ["Kakariko Well - Bottom", true, new string[] {  }],

        ["Blind's Hideout - Top", true, new string[] {  }],

        ["Blind's Hideout - Left", true, new string[] {  }],

        ["Blind's Hideout - Right", true, new string[] {  }],

        ["Blind's Hideout - Far Left", true, new string[] {  }],

        ["Blind's Hideout - Far Right", true, new string[] {  }],

        ["Bonk Rocks Item", false, new string[] {  }],
        ["Bonk Rocks Item", true, new string[] { "PegasusBoots" }],

        ["Mini Moldorm Cave - Far Left", true, new string[] {  }],

        ["Mini Moldorm Cave - Left", true, new string[] {  }],

        ["Mini Moldorm Cave - Right", true, new string[] {  }],

        ["Mini Moldorm Cave - Far Right", true, new string[] {  }],

        ["Ice Rod Cave Item", true, new string[] {  }],

        ["Bottle Merchant", true, new string[] {  }],

        ["Sahasrahla's Hut - Sahasrahla", false, new string[] {  }],
        ["Sahasrahla's Hut - Sahasrahla", true, new string[] { "PendantOfCourage" }],

        ["Magic Bat Item", false, new string[] {  }],
        ["Magic Bat Item", true, new string[] { "Powder" }],

        ["Sick Kid Item", false, new string[] {  }],
        ["Sick Kid Item", true, new string[] { "BottleWithBee" }],
        ["Sick Kid Item", true, new string[] { "BottleWithFairy" }],
        ["Sick Kid Item", true, new string[] { "BottleWithRedPotion" }],
        ["Sick Kid Item", true, new string[] { "BottleWithGreenPotion" }],
        ["Sick Kid Item", true, new string[] { "BottleWithBluePotion" }],
        ["Sick Kid Item", true, new string[] { "Bottle" }],
        ["Sick Kid Item", true, new string[] { "BottleWithGoldBee" }],

        ["Hobo", true, new string[] {  }],

        ["Bombos Tablet", false, new string[] {  }],
        ["Bombos Tablet", true, new string[] { "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Bombos Tablet", true, new string[] { "BookOfMudora", "L2Sword" }],
        ["Bombos Tablet", true, new string[] { "BookOfMudora", "L3Sword" }],
        ["Bombos Tablet", true, new string[] { "BookOfMudora", "L4Sword" }],

        ["King Zora", true, new string[] {  }],

        ["Lost Woods Hideout Item", true, new string[] {  }],

        ["Lumberjack Tree", false, new string[] {  }],
        ["Lumberjack Tree", true, new string[] { "PegasusBoots", "DefeatAgahnim" }],

        ["Cave 45 Item", true, new string[] {  }],

        ["Graveyard Ledge", true, new string[] {  }],

        ["Checkerboard Cave Item", false, new string[] {  }],
        ["Checkerboard Cave Item", true, new string[] { "ProgressiveGlove" }],
        ["Checkerboard Cave Item", true, new string[] { "PowerGlove" }],
        ["Checkerboard Cave Item", true, new string[] { "TitansMitt" }],

        ["Mini Moldorm Cave - NPC", true, new string[] {  }],

        ["Library", false, new string[] {  }],
        ["Library", true, new string[] { "PegasusBoots" }],

        ["Mushroom", true, new string[] {  }],

        ["Potion Shop", false, new string[] {  }],
        ["Potion Shop", true, new string[] { "Mushroom" }],

        ["Maze Race", true, new string[] {  }],

        ["Desert Ledge", true, new string[] {  }],

        ["Lake Hylia Island", true, new string[] {  }],

        ["Sunken Treasure", true, new string[] {  }],

        ["Zora's Domain Ledge Item", false, new string[] {  }],
        ["Zora's Domain Ledge Item", true, new string[] { "Flippers" }],
        ["Zora's Domain Ledge Item", true, new string[] { "PegasusBoots" }],

        ["Flute Spot", false, new string[] {  }],
        ["Flute Spot", true, new string[] { "Shovel" }],

        ["Waterfall Fairy - Left", false, new string[] {  }],
        ["Waterfall Fairy - Left", true, new string[] { "Flippers" }],
        ["Waterfall Fairy - Left", true, new string[] { "MoonPearl" }],
        ["Waterfall Fairy - Left", true, new string[] { "PegasusBoots" }],

        ["Waterfall Fairy - Right", false, new string[] {  }],
        ["Waterfall Fairy - Right", true, new string[] { "Flippers" }],
        ["Waterfall Fairy - Right", true, new string[] { "MoonPearl" }],
        ["Waterfall Fairy - Right", true, new string[] { "PegasusBoots" }],
    ];
}
