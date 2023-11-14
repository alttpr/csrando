namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class LightWorldTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Master Sword Pedestal", false, new string[] {  }],
        ["Master Sword Pedestal", true, new string[] { "PendantOfCourage", "PendantOfWisdom", "PendantOfPower" }],

        ["Link's Uncle", true, new string[] {  }],

        ["Secret Passage", true, new string[] { "UncleSword" }],

        ["King's Tomb Chest", false, new string[] {  }],
        ["King's Tomb Chest", true, new string[] { "PegasusBoots", "ProgressiveGlove", "ProgressiveGlove" }],
        ["King's Tomb Chest", true, new string[] { "PegasusBoots", "TitansMitt" }],
        ["King's Tomb Chest", true, new string[] { "PegasusBoots", "ProgressiveGlove", "Hammer", "MoonPearl", "MagicMirror" }],
        ["King's Tomb Chest", true, new string[] { "PegasusBoots", "PowerGlove", "Hammer", "MoonPearl", "MagicMirror" }],
        ["King's Tomb Chest", true, new string[] { "PegasusBoots", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "MoonPearl", "MagicMirror" }],
        ["King's Tomb Chest", true, new string[] { "PegasusBoots", "AgahnimDefeated", "PowerGlove", "Hookshot", "MoonPearl", "MagicMirror" }],
        ["King's Tomb Chest", true, new string[] { "PegasusBoots", "AgahnimDefeated", "Hammer", "Hookshot", "MoonPearl", "MagicMirror" }],
        ["King's Tomb Chest", true, new string[] { "PegasusBoots", "AgahnimDefeated", "Flippers", "Hookshot", "MoonPearl", "MagicMirror" }],

        ["Floodgate Chest", true, new string[] {  }],

        ["Link's House - Chest", true, new string[] {  }],

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

        ["Bottle Merchant Item", true, new string[] {  }],

        ["Sahasrahla's Hut - Sahasrahla", false, new string[] {  }],
        ["Sahasrahla's Hut - Sahasrahla", true, new string[] { "PendantOfCourage" }],

        ["Magic Bat Item", false, new string[] {  }],
        ["Magic Bat Item", true, new string[] { "Powder", "Hammer" }],
        ["Magic Bat Item", true, new string[] { "Powder", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "MagicMirror" }],
        ["Magic Bat Item", true, new string[] { "Powder", "TitansMitt", "MoonPearl", "MagicMirror" }],

        ["Sick Kid Item", false, new string[] {  }],
        ["Sick Kid Item", true, new string[] { "BottleWithBee" }],
        ["Sick Kid Item", true, new string[] { "BottleWithFairy" }],
        ["Sick Kid Item", true, new string[] { "BottleWithRedPotion" }],
        ["Sick Kid Item", true, new string[] { "BottleWithGreenPotion" }],
        ["Sick Kid Item", true, new string[] { "BottleWithBluePotion" }],
        ["Sick Kid Item", true, new string[] { "Bottle" }],
        ["Sick Kid Item", true, new string[] { "BottleWithGoldBee" }],

        ["Bomb Hut", true, new string[] { }],

        ["Hobo", false, new string[] {  }],
        ["Hobo", true, new string[] { "Flippers" }],

        ["Bombos Tablet", false, new string[] {  }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "TitansMitt" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "TitansMitt" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "TitansMitt" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "TitansMitt" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "ProgressiveGlove", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "ProgressiveGlove", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "ProgressiveGlove", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "ProgressiveGlove", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "PowerGlove", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "PowerGlove", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "PowerGlove", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "PowerGlove", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "AgahnimDefeated", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "AgahnimDefeated", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "AgahnimDefeated", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "AgahnimDefeated", "Hammer" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "AgahnimDefeated", "Flippers", "Hookshot" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "AgahnimDefeated", "Flippers", "Hookshot" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "AgahnimDefeated", "Flippers", "Hookshot" }],
        ["Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["King Zora", false, new string[] {  }],
        ["King Zora", true, new string[] { "Flippers" }],
        ["King Zora", true, new string[] { "ProgressiveGlove" }],
        ["King Zora", true, new string[] { "PowerGlove" }],
        ["King Zora", true, new string[] { "TitansMitt" }],

        ["Lost Woods Hideout Item", true, new string[] {  }],

        ["Lumberjack Tree", false, new string[] {  }],
        ["Lumberjack Tree", true, new string[] { "PegasusBoots", "AgahnimDefeated" }],

        ["Cave 45 Item", false, new string[] {  }],
        ["Cave 45 Item", true, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Cave 45 Item", true, new string[] { "MoonPearl", "MagicMirror", "TitansMitt" }],
        ["Cave 45 Item", true, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "Hammer" }],
        ["Cave 45 Item", true, new string[] { "MoonPearl", "MagicMirror", "PowerGlove", "Hammer" }],
        ["Cave 45 Item", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "Hammer" }],
        ["Cave 45 Item", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Cave 45 Item", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Graveyard Cave Item", false, new string[] {  }],
        ["Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "TitansMitt" }],
        ["Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "Hammer" }],
        ["Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "PowerGlove", "Hammer" }],
        ["Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "Hammer", "Hookshot" }],
        ["Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Checkerboard Cave Item", false, new string[] {  }],
        ["Checkerboard Cave Item", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Checkerboard Cave Item", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt" }],

        ["Mini Moldorm Cave - NPC", true, new string[] {  }],

        ["Library Item", false, new string[] {  }],
        ["Library Item", true, new string[] { "PegasusBoots" }],

        ["Mushroom", true, new string[] {  }],

        ["Potion Shop Item", false, new string[] {  }],
        ["Potion Shop Item", true, new string[] { "Mushroom" }],

        ["Maze Race", true, new string[] {  }],

        ["Desert Ledge - Item", false, new string[] {  }],
        ["Desert Ledge - Item", true, new string[] { "BookOfMudora" }],
        ["Desert Ledge - Item", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Desert Ledge - Item", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt" }],

        ["Lake Hylia Island Item", false, new string[] {  }],
        ["Lake Hylia Island Item", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Lake Hylia Island Item", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "TitansMitt" }],
        ["Lake Hylia Island Item", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "ProgressiveGlove", "Hammer" }],
        ["Lake Hylia Island Item", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "PowerGlove", "Hammer" }],
        ["Lake Hylia Island Item", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "AgahnimDefeated" }],

        ["Sunken Treasure", true, new string[] {  }],

        ["Zora's Domain Ledge Item", false, new string[] {  }],
        ["Zora's Domain Ledge Item", true, new string[] { "Flippers" }],

        ["Flute Spot", false, new string[] {  }],
        ["Flute Spot", true, new string[] { "Shovel" }],

        ["Waterfall Fairy - Left", false, new string[] {  }],
        ["Waterfall Fairy - Left", true, new string[] { "Flippers" }],

        ["Waterfall Fairy - Right", false, new string[] {  }],
        ["Waterfall Fairy - Right", true, new string[] { "Flippers" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
