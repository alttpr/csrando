namespace RandomizerTests.Games.Zelda1;

using System.Linq;
using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;
using Graph = Randomizer.Graph.Graph;
using Vertex = Randomizer.Graph.Vertex;

[TestClass]
public sealed class ShopShuffleTest
{
    // The farming weapons the shop logic gates on (see YamlReader.GetFarmingWeapons).
    private static readonly string[] ExpectedFarmingWeapons =
        ["SwordL1", "SwordL2", "SwordL3", "Rod", "RedCandle"];

    private static World CreateWorld(ShopShuffleOption shop, int seed = 42)
    {
        var z1Config = new Config
        {
            ShopShuffle = shop,
            Triforces = "8",
        };
        z1Config.SelectRandomValues(new PRNG(seed));
        var worldConfig = new WorldConfig { Zelda1 = z1Config };
        var world = new World(1, worldConfig, new Graph(), new PRNG(seed));
        world.Graph.SetVertexIds(); // normally done by the GameRandomizer base after world creation
        return world;
    }

    // Run the full randomizer (assumed fill) and return the resulting world.
    private static World RandomizeWorld(ShopShuffleOption shop, int seed)
    {
        var z1Config = new Config { ShopShuffle = shop, Triforces = "8" };
        z1Config.SelectRandomValues(new PRNG(seed));
        var randomizer = new Randomizer.Games.Zelda1.GameRandomizer(
            [new WorldConfig { Zelda1 = z1Config }], new PRNG(seed));
        randomizer.Randomize();
        return (World)randomizer.Worlds[0];
    }

    private static System.Collections.Generic.List<Vertex> ShopLocations(World world) =>
        world.GetLocationsOfType(VertexType.Item)
            .Where(v => v.Name.Contains(" - Shop - "))
            .ToList();

    private static System.Collections.Generic.List<Vertex> TakeAnyLocations(World world) =>
        world.GetLocationsOfType(VertexType.Item)
            .Where(v => v.Name.Contains(" - Take Any Item - "))
            .ToList();

    // Count-accumulating items that must never appear in a repeatable (multi-purchase) shop. In
    // standalone Z1 only HeartContainer accumulates (a Heart refill is fine to rebuy).
    private static readonly string[] CountItems = ["HeartContainer"];

    // Keep in sync with ItemPooler.ShopJunkItems. These are the only items safe to put in a
    // take-any cave, where choosing one item permanently removes the other choices.
    private static readonly string[] TakeAnyItems =
        ["RedPotion", "BluePotion", "BlueRing", "Bombs", "Arrows", "Rupee", "Rupee5", "Heart", "Key", "MagicShield"];

    // A cave id parsed from a shop/take-any location name.
    private static int CaveIdOf(Vertex v) => System.Convert.ToInt32(v.Name.Split(' ')[1], 16);

    [TestMethod]
    public void ShopShuffleOff_CreatesNoShopLocations()
    {
        var world = CreateWorld(ShopShuffleOption.Off);
        Assert.AreEqual(0, ShopLocations(world).Count,
            "With shop shuffle off, shops keep their vanilla contents and are not graph locations.");
    }

    [TestMethod]
    [DataRow(ShopShuffleOption.Full)]
    [DataRow(ShopShuffleOption.Junk)]
    public void ShopShuffleOn_CreatesShopLocations(ShopShuffleOption mode)
    {
        var world = CreateWorld(mode);
        Assert.IsTrue(ShopLocations(world).Count > 0,
            $"Shop shuffle {mode} should expose shop slots as item locations.");
    }

    [TestMethod]
    public void FullShops_GateItemsBehindFarmingWeapon()
    {
        var world = CreateWorld(ShopShuffleOption.Full);
        var shopLocations = ShopLocations(world);
        Assert.IsTrue(shopLocations.Count > 0, "Expected at least one shop location.");

        foreach (var shopItem in shopLocations)
        {
            // Every path INTO a shop item should be conditioned on a farming weapon (the OR is
            // expressed as parallel edges), and never be unconditional ("fixed").
            var incoming = world.Graph.GetVertices()
                .SelectMany(v => v.Edges)
                .Where(e => e.To == shopItem)
                .ToList();

            Assert.IsTrue(incoming.Count > 0, $"{shopItem.Name} should have incoming edges.");
            Assert.IsTrue(incoming.All(e => ExpectedFarmingWeapons.Contains(e.Condition.Item.Name)),
                $"{shopItem.Name} should only be reachable via a farming weapon, " +
                $"found: {string.Join(",", incoming.Select(e => e.Condition.Item.Name))}");

            // Each expected weapon should provide a path (full OR coverage).
            var weaponsOnEdges = incoming.Select(e => e.Condition.Item.Name).ToHashSet();
            CollectionAssert.AreEquivalent(ExpectedFarmingWeapons, weaponsOnEdges.ToArray(),
                $"{shopItem.Name} should be reachable via each farming weapon.");
        }
    }

    [TestMethod]
    public void FullShops_ProgressionNotReachableWithoutAWeapon()
    {
        var world = CreateWorld(ShopShuffleOption.Full);
        var shopItem = ShopLocations(world).First();

        // Search from the start with only "fixed" (no weapon at all). Shop items must not be
        // reachable, since rupees can't be farmed without a usable weapon.
        var inventory = new Inventory([world.GetItem("fixed")]);
        var searcher = new Searcher(world.Graph, world.Start!, inventory);

        Assert.IsFalse(searcher.HasVisited(shopItem),
            "A shop item should be out of logic when the player has no farming weapon.");
    }

    [TestMethod]
    public void FullShops_ReachableOnceAWeaponIsHeld()
    {
        var world = CreateWorld(ShopShuffleOption.Full);

        // With a sword (farming enabled), at least one shop item should come into logic.
        var inventory = new Inventory([world.GetItem("fixed"), world.GetItem("SwordL1")]);
        var searcher = new Searcher(world.Graph, world.Start!, inventory);

        bool anyReachable = ShopLocations(world).Any(searcher.HasVisited);
        Assert.IsTrue(anyReachable,
            "With a sword, at least one shop item should be reachable (farming enabled).");
    }

    // End-to-end: the result is winnable and every item location (shop slots included) is filled.
    [TestMethod]
    [DataRow(ShopShuffleOption.Full, 42)]
    [DataRow(ShopShuffleOption.Full, 7)]
    [DataRow(ShopShuffleOption.Junk, 42)]
    [DataRow(ShopShuffleOption.Junk, 7)]
    [TestCategory(TestCategories.Slow)]
    public void ShopShuffle_FullFill_IsWinnableAndFillsEveryLocation(ShopShuffleOption shop, int seed)
    {
        var z1Config = new Config { ShopShuffle = shop, Triforces = "8" };
        z1Config.SelectRandomValues(new PRNG(seed));
        var randomizer = new Randomizer.Games.Zelda1.GameRandomizer(
            [new WorldConfig { Zelda1 = z1Config }], new PRNG(seed));

        randomizer.Randomize();

        Assert.IsTrue(randomizer.IsWinnable(), $"Shop shuffle {shop} seed {seed} should be winnable.");

        var world = (World)randomizer.Worlds[0];
        int empty = world.GetLocationsOfType(VertexType.Item).Count(v => v.Item == null);
        Assert.AreEqual(0, empty, $"Shop shuffle {shop} seed {seed}: every item location should be filled.");

        // Junk-mode shop slots must never hold progression items.
        if (shop == ShopShuffleOption.Junk)
        {
            var progression = new[] { "SwordL1", "SwordL2", "SwordL3", "Raft", "StepLadder", "Recorder", "Bow", "Rod", "PowerBracelet", "MagicalKey" };
            foreach (var loc in ShopLocations(world))
            {
                Assert.IsFalse(progression.Contains(loc.Item?.Name),
                    $"Junk shop slot {loc.Name} should not hold progression item {loc.Item?.Name}.");
            }
        }
    }

    [TestMethod]
    [DataRow(ShopShuffleOption.Off)]
    [DataRow(ShopShuffleOption.Junk)]
    [DataRow(ShopShuffleOption.Full)]
    [TestCategory(TestCategories.Slow)]
    public void TakeAnys_AreJunkOnly_NeverProgression(ShopShuffleOption shop)
    {
        // Take-anys let the player pick one of several items and lose the rest, so they must never
        // hold anything outside the explicitly safe junk list (otherwise the player can strand it).

        for (int seed = 1; seed <= 5; seed++)
        {
            var world = RandomizeWorld(shop, seed);
            foreach (var loc in TakeAnyLocations(world))
            {
                Assert.IsTrue(TakeAnyItems.Contains(loc.Item?.Name),
                    $"shop {shop}, seed {seed}: take-any {loc.Name} should not hold unsafe item {loc.Item?.Name}.");
            }
        }
    }

    // The vanilla Level 7 Hungry Goriya room (map A8): the Goriya blocks the room until
    // "defeated" by handing over Bait (its kill_items), so every exit is gated on the
    // standard room-clear chain while entry and the room interior stay free.
    private const string GoriyaRoom = "Underworld - Level 7 - Map A8";

    [TestMethod]
    [DataRow(ShopShuffleOption.Off)]
    [DataRow(ShopShuffleOption.Full)]
    [DataRow(ShopShuffleOption.Junk)]
    public void HungryGoriyaRoom_ExitsGatedOnRoomClear(ShopShuffleOption mode)
    {
        var world = CreateWorld(mode);
        var allEdges = world.Graph.GetVertices().SelectMany(v => v.Edges).ToList();

        // Leaving through any door requires the room clear (A8: Open north, Locked south)...
        var exitEdges = allEdges
            .Where(e => e.From.Name.StartsWith($"{GoriyaRoom} - ") && e.From.Name.EndsWith(" edge - Exit"))
            .ToList();
        Assert.IsTrue(exitEdges.Count > 0, "Expected exit edges in the Hungry Goriya room.");
        Assert.IsTrue(exitEdges.All(e => e.Condition.Item.Name == "MapA8Clear"),
            "Leaving the Hungry Goriya room must require the room clear, found: " +
            string.Join(",", exitEdges.Select(e => e.Condition.Item.Name)));

        // ...with the locked south door still needing its Key on top of the clear (the AND is
        // sequenced through the "- Cleared" node).
        var keyEdges = allEdges
            .Where(e => e.From.Name == $"{GoriyaRoom} - Bottom edge - Exit - Cleared")
            .ToList();
        Assert.IsTrue(keyEdges.Count > 0 && keyEdges.All(e => e.Condition.Item.Name == "Key"),
            "The locked south door must still require a Key after the clear.");

        // ...and the clear chain requires defeating the Goriya, i.e. holding Bait.
        var defeatEdges = allEdges
            .Where(e => e.To.Name == $"{GoriyaRoom} - Clear - Defeated Hungry Goriya")
            .ToList();
        Assert.IsTrue(defeatEdges.Count > 0 && defeatEdges.All(e => e.Condition.Item.Name == "DefeatHungry Goriya"),
            "The Goriya room clear must require defeating the Hungry Goriya.");

        // Entry stays free: the entrance-to-room step must not be gated, or the clear chain
        // (fed from the room interior) could never be satisfied.
        var entryEdges = allEdges
            .Where(e => e.To.Name == $"{GoriyaRoom} - Top edge"
                        && e.From.Name == $"{GoriyaRoom} - Top edge - Entrance")
            .ToList();
        Assert.IsTrue(entryEdges.Count > 0 && entryEdges.All(e => e.Condition.Item.Name == "fixed"),
            "Entering the Hungry Goriya room must stay free.");
    }

    [TestMethod]
    public void ShopShuffleOff_BaitPurchasableAtVanillaShops()
    {
        // With vanilla shops the Bait-selling caves (1F and 20) are modeled as a purchasable
        // Bait, gated on a farming weapon like any shop purchase — that's what satisfies the
        // Goriya gate when shops keep their vanilla wares.
        var world = CreateWorld(ShopShuffleOption.Off);
        var baitNodes = world.Graph.GetVertices()
            .Where(v => v.Name.EndsWith(" - Shop - Bait"))
            .ToList();
        Assert.AreEqual(2, baitNodes.Count, "Expected the two vanilla Bait shops (caves 1F and 20).");

        foreach (var baitNode in baitNodes)
        {
            var incoming = world.Graph.GetVertices()
                .SelectMany(v => v.Edges)
                .Where(e => e.To == baitNode)
                .ToList();
            Assert.IsTrue(incoming.Count > 0 && incoming.All(e => ExpectedFarmingWeapons.Contains(e.Condition.Item.Name)),
                $"{baitNode.Name} should be gated on a farming weapon.");
        }

        // No weapon -> no rupee farming -> no Bait purchase in logic.
        var noWeapon = new Searcher(world.Graph, world.Start!, new Inventory([world.GetItem("fixed")]));
        Assert.IsFalse(baitNodes.Any(noWeapon.HasVisited),
            "Bait should be out of logic without a farming weapon.");

        var withSword = new Searcher(world.Graph, world.Start!,
            new Inventory([world.GetItem("fixed"), world.GetItem("SwordL1")]));
        Assert.IsTrue(baitNodes.Any(withSword.HasVisited),
            "With a sword, buying Bait should be in logic.");
    }

    [TestMethod]
    [DataRow(ShopShuffleOption.Full)]
    [DataRow(ShopShuffleOption.Junk)]
    [TestCategory(TestCategories.Slow)]
    public void ShopShuffleOn_BaitIsStockedInARepeatableShop(ShopShuffleOption mode)
    {
        // Shop shuffle replaces the vanilla Bait wares, so Bait is force-stocked in a repeatable
        // shop (cave id < 0x24): the Goriya consumes the Bait on each feeding, so the source must
        // restock or multiple Goriya rooms could never be supported.
        for (int seed = 1; seed <= 5; seed++)
        {
            var world = RandomizeWorld(mode, seed);
            var baitLocation = world.GetLocationsOfType(VertexType.Item)
                .FirstOrDefault(v => v.Item?.Name == "Bait");
            Assert.IsNotNull(baitLocation, $"shop {mode}, seed {seed}: Bait should be placed in the world.");
            Assert.IsTrue(baitLocation.Name.Contains(" - Shop - ") && CaveIdOf(baitLocation) < 0x24,
                $"shop {mode}, seed {seed}: Bait should sit in a repeatable shop, found {baitLocation.Name}.");
        }
    }

    [TestMethod]
    public void BuyOnceShops_HaveExactlyOneItem_OnFreshCaveIds()
    {
        var world = CreateWorld(ShopShuffleOption.Full);

        // Buy-once shops are synthesized on fresh cave IDs (>= 0x24). Each must expose exactly one
        // shop item location, so the player can't take the wrong one.
        var buyOnceShopItems = ShopLocations(world).Where(v => CaveIdOf(v) >= 0x24).ToList();
        Assert.IsTrue(buyOnceShopItems.Count > 0, "Expected at least one buy-once shop (fresh cave id).");

        var perCave = buyOnceShopItems.GroupBy(CaveIdOf);
        foreach (var caveGroup in perCave)
        {
            Assert.AreEqual(1, caveGroup.Count(),
                $"Buy-once cave {caveGroup.Key:X2} must have exactly one item location.");
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Slow)]
    public void RepeatableShops_NeverHoldCountItems()
    {
        // Repeatable (multi-purchase) shops use existing cave IDs < 0x24. Because purchases
        // repeat, they must not sell count-accumulating items.
        for (int seed = 1; seed <= 5; seed++)
        {
            var world = RandomizeWorld(ShopShuffleOption.Full, seed);
            var repeatableShopItems = ShopLocations(world).Where(v => CaveIdOf(v) < 0x24);
            foreach (var loc in repeatableShopItems)
            {
                Assert.IsFalse(CountItems.Contains(loc.Item?.Name),
                    $"seed {seed}: repeatable shop {loc.Name} must not hold count item {loc.Item?.Name}.");
            }
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Slow)]
    public void ConsumableCaves_HaveNoDuplicateItemsWithinACave()
    {
        // Repeatable shops and take-anys draw distinct consumables per cave: two slots in the same
        // cave must never hold the same item.
        for (int seed = 1; seed <= 6; seed++)
        {
            var world = RandomizeWorld(ShopShuffleOption.Full, seed);

            // Group multi-slot consumable caves (repeatable shops < 0x24, and take-anys) by cave id.
            var consumableLocs = world.GetLocationsOfType(VertexType.Item)
                .Where(v => (v.Name.Contains(" - Shop - ") && CaveIdOf(v) < 0x24)
                            || v.Name.Contains(" - Take Any Item - "));

            foreach (var caveGroup in consumableLocs.GroupBy(CaveIdOf).Where(g => g.Count() > 1))
            {
                var items = caveGroup.Select(v => v.Item?.Name).ToList();
                CollectionAssert.AllItemsAreUnique(items,
                    $"seed {seed}: cave {caveGroup.Key:X2} has duplicate items: {string.Join(",", items)}");
            }
        }
    }
}
