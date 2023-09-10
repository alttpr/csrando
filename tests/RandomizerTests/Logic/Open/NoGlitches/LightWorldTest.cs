using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class LightWorldTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Master Sword Pedestal", false, new string[] {  } },
        new object[] { "Master Sword Pedestal", true, new string[] { "PendantOfCourage", "PendantOfWisdom", "PendantOfPower" } },

        new object[] { "Link's Uncle", true, new string[] {  } },

        new object[] { "Secret Passage", true, new string[] { "UncleSword" } },

        new object[] { "King's Tomb Chest", false, new string[] {  } },
        new object[] { "King's Tomb Chest", true, new string[] { "PegasusBoots", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "King's Tomb Chest", true, new string[] { "PegasusBoots", "TitansMitt" } },
        new object[] { "King's Tomb Chest", true, new string[] { "PegasusBoots", "ProgressiveGlove", "Hammer", "MoonPearl", "MagicMirror" } },
        new object[] { "King's Tomb Chest", true, new string[] { "PegasusBoots", "PowerGlove", "Hammer", "MoonPearl", "MagicMirror" } },
        new object[] { "King's Tomb Chest", true, new string[] { "PegasusBoots", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "MoonPearl", "MagicMirror" } },
        new object[] { "King's Tomb Chest", true, new string[] { "PegasusBoots", "AgahnimDefeated", "PowerGlove", "Hookshot", "MoonPearl", "MagicMirror" } },
        new object[] { "King's Tomb Chest", true, new string[] { "PegasusBoots", "AgahnimDefeated", "Hammer", "Hookshot", "MoonPearl", "MagicMirror" } },
        new object[] { "King's Tomb Chest", true, new string[] { "PegasusBoots", "AgahnimDefeated", "Flippers", "Hookshot", "MoonPearl", "MagicMirror" } },

        new object[] { "Floodgate Chest", true, new string[] {  } },

        new object[] { "Link's House - Chest", true, new string[] {  } },

        new object[] { "Kakariko Tavern", true, new string[] {  } },

        new object[] { "Chicken House", true, new string[] {  } },

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

        new object[] { "Pegasus Rocks", false, new string[] {  } },
        new object[] { "Pegasus Rocks", true, new string[] { "PegasusBoots" } },

        new object[] { "Mini Moldorm Cave - Far Left", true, new string[] {  } },

        new object[] { "Mini Moldorm Cave - Left", true, new string[] {  } },

        new object[] { "Mini Moldorm Cave - Right", true, new string[] {  } },

        new object[] { "Mini Moldorm Cave - Far Right", true, new string[] {  } },

        new object[] { "Ice Rod Cave", true, new string[] {  } },

        new object[] { "Bottle Merchant", true, new string[] {  } },

        new object[] { "Sahasrahla's Hut - Sahasrahla", false, new string[] {  } },
        new object[] { "Sahasrahla's Hut - Sahasrahla", true, new string[] { "PendantOfCourage" } },

        new object[] { "Magic Bat", false, new string[] {  } },
        new object[] { "Magic Bat", true, new string[] { "Powder", "Hammer" } },
        new object[] { "Magic Bat", true, new string[] { "Powder", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "MagicMirror" } },
        new object[] { "Magic Bat", true, new string[] { "Powder", "TitansMitt", "MoonPearl", "MagicMirror" } },

        new object[] { "Sick Kid", false, new string[] {  } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithBee" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithFairy" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Sick Kid", true, new string[] { "Bottle" } },
        new object[] { "Sick Kid", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Hobo", false, new string[] {  } },
        new object[] { "Hobo", true, new string[] { "Flippers" } },

        new object[] { "Bombos Tablet", false, new string[] {  } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "TitansMitt" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "TitansMitt" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "TitansMitt" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "TitansMitt" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "ProgressiveGlove", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "ProgressiveGlove", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "ProgressiveGlove", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "ProgressiveGlove", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "PowerGlove", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "PowerGlove", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "PowerGlove", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "PowerGlove", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "AgahnimDefeated", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "AgahnimDefeated", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "AgahnimDefeated", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "AgahnimDefeated", "Hammer" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword", "AgahnimDefeated", "Flippers", "Hookshot" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L2Sword", "AgahnimDefeated", "Flippers", "Hookshot" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L3Sword", "AgahnimDefeated", "Flippers", "Hookshot" } },
        new object[] { "Bombos Tablet", true, new string[] { "MoonPearl", "MagicMirror", "BookOfMudora", "L4Sword", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "King Zora", false, new string[] {  } },
        new object[] { "King Zora", true, new string[] { "Flippers" } },
        new object[] { "King Zora", true, new string[] { "ProgressiveGlove" } },
        new object[] { "King Zora", true, new string[] { "PowerGlove" } },
        new object[] { "King Zora", true, new string[] { "TitansMitt" } },

        new object[] { "Lost Woods Hideout", true, new string[] {  } },

        new object[] { "Lumberjack Tree", false, new string[] {  } },
        new object[] { "Lumberjack Tree", true, new string[] { "PegasusBoots", "AgahnimDefeated" } },

        new object[] { "Cave 45", false, new string[] {  } },
        new object[] { "Cave 45", true, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Cave 45", true, new string[] { "MoonPearl", "MagicMirror", "TitansMitt" } },
        new object[] { "Cave 45", true, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "Hammer" } },
        new object[] { "Cave 45", true, new string[] { "MoonPearl", "MagicMirror", "PowerGlove", "Hammer" } },
        new object[] { "Cave 45", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "Hammer" } },
        new object[] { "Cave 45", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Cave 45", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Graveyard Cave Item", false, new string[] {  } },
        new object[] { "Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "TitansMitt" } },
        new object[] { "Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "Hammer" } },
        new object[] { "Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "PowerGlove", "Hammer" } },
        new object[] { "Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Graveyard Cave Item", true, new string[] { "MoonPearl", "MagicMirror", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Checkerboard Cave", false, new string[] {  } },
        new object[] { "Checkerboard Cave", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Checkerboard Cave", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt" } },

        new object[] { "Mini Moldorm Cave - NPC", true, new string[] {  } },

        new object[] { "Library", false, new string[] {  } },
        new object[] { "Library", true, new string[] { "PegasusBoots" } },

        new object[] { "Mushroom", true, new string[] {  } },

        new object[] { "Potion Shop Item", false, new string[] {  } },
        new object[] { "Potion Shop Item", true, new string[] { "Mushroom" } },

        new object[] { "Maze Race", true, new string[] {  } },

        new object[] { "Desert Ledge - Item", false, new string[] {  } },
        new object[] { "Desert Ledge - Item", true, new string[] { "BookOfMudora" } },
        new object[] { "Desert Ledge - Item", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Desert Ledge - Item", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt" } },

        new object[] { "Lake Hylia Island", false, new string[] {  } },
        new object[] { "Lake Hylia Island", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Lake Hylia Island", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "TitansMitt" } },
        new object[] { "Lake Hylia Island", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "ProgressiveGlove", "Hammer" } },
        new object[] { "Lake Hylia Island", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "PowerGlove", "Hammer" } },
        new object[] { "Lake Hylia Island", true, new string[] { "Flippers", "MoonPearl", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Sunken Treasure", true, new string[] {  } },

        new object[] { "Zora's Domain Ledge Item", false, new string[] {  } },
        new object[] { "Zora's Domain Ledge Item", true, new string[] { "Flippers" } },

        new object[] { "Flute Spot", false, new string[] {  } },
        new object[] { "Flute Spot", true, new string[] { "Shovel" } },

        new object[] { "Waterfall Fairy - Left", false, new string[] {  } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "Flippers" } },

        new object[] { "Waterfall Fairy - Right", false, new string[] {  } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "Flippers" } },
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
