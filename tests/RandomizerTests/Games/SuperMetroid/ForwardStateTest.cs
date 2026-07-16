namespace RandomizerTests.Games.SuperMetroid;

using Randomizer.Games;
using Randomizer.Games.SuperMetroid;
using Randomizer.Graph;

[TestClass]
public sealed class ForwardStateTest
{
    [TestMethod]
    public void DebtState_MatchesRemainingResourcesAcrossCostsAndRefills()
    {
        var (world, inventory) = CreateWorldAndInventory();
        var remaining = FullState(world, inventory);
        var debt = ForwardState.FromVisited(remaining, inventory, world);
        var context = new SearchContext(inventory, world);
        RequirementCost[] costs =
        [
            new() { Energy = 100, Missiles = 3, SuperMissiles = 2, PowerBombs = 1 },
            new() { Energy = -50, Missiles = -2 },
            new() { Energy = -99999, SuperMissiles = -99999, PowerBombs = -99999 },
            new() { Energy = 99, Missiles = 2, PowerBombs = 1 },
        ];

        foreach (var cost in costs)
        {
            remaining = remaining.ApplyCost(cost, inventory, world)!.Value;
            debt = debt.ApplyCost(cost, context)!.Value;
            AssertEquivalent(remaining, debt.ToVisited(context));
        }
    }

    [TestMethod]
    public void DebtState_MatchesDrainAndMissileSubstitution()
    {
        var (world, inventory) = CreateWorldAndInventory();
        var remaining = FullState(world, inventory) with { Missiles = 1 };
        var debt = ForwardState.FromVisited(remaining, inventory, world);
        var context = new SearchContext(inventory, world);
        RequirementCost[] costs =
        [
            new() { Missiles = 4 },
            new() { Energy = 80 | 0x8000, SuperMissiles = 3 | 0x8000 },
        ];

        foreach (var cost in costs)
        {
            remaining = remaining.ApplyCost(cost, inventory, world)!.Value;
            debt = debt.ApplyCost(cost, context)!.Value;
            AssertEquivalent(remaining, debt.ToVisited(context));
        }
    }

    [TestMethod]
    public void DebtState_GainsCapacityWithoutBeingRewritten()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        var inventory = world.ComputeStartingItems();
        inventory.AddItem(world.GetItem("Missile"));
        var originalContext = new SearchContext(inventory, world);
        var state = ForwardState.Empty.ApplyCost(
            new RequirementCost { Missiles = 5 }, originalContext)!.Value;

        inventory.AddItem(world.GetItem("Missile"));
        var expandedContext = new SearchContext(inventory, world);

        Assert.AreEqual(5, state.ToVisited(expandedContext).Missiles);
    }

    [TestMethod]
    public void Frontier_PreservesDominanceAcrossInlineAndOverflowStates()
    {
        var frontier = new ForwardStateFrontier();
        for (int energy = 0; energy < 6; energy++)
        {
            Assert.IsTrue(frontier.AddNondominated(new ForwardState(
                energy, 5 - energy, 0, 0, 0, 0)));
        }

        Assert.AreEqual(6, frontier.Count);
        var best = ForwardState.Empty;
        Assert.IsTrue(frontier.AddNondominated(best));
        Assert.AreEqual(1, frontier.Count);
        Assert.IsFalse(frontier.AddNondominated(best));

        frontier.RemoveEqual(best);
        Assert.IsTrue(frontier.IsEmpty);
    }

    private static (World World, Inventory Inventory) CreateWorldAndInventory()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        var inventory = world.ComputeStartingItems();
        inventory.AddItem(world.GetItem("ETank"), 2);
        inventory.AddItem(world.GetItem("Missile"), 2);
        inventory.AddItem(world.GetItem("Super"), 2);
        inventory.AddItem(world.GetItem("PowerBomb"), 2);
        return (world, inventory);
    }

    private static VisitedState FullState(World world, Inventory inventory) => new()
    {
        Energy = 99 + inventory.GetCount(world.GetItem("ETank")) * 100,
        Missiles = inventory.GetCount(world.GetItem("Missile")) * 5,
        SuperMissiles = inventory.GetCount(world.GetItem("Super")) * 5,
        PowerBombs = inventory.GetCount(world.GetItem("PowerBomb")) * 5,
    };

    private static void AssertEquivalent(VisitedState expected, VisitedState actual)
    {
        Assert.AreEqual(expected.Energy, actual.Energy);
        Assert.AreEqual(expected.Missiles, actual.Missiles);
        Assert.AreEqual(expected.SuperMissiles, actual.SuperMissiles);
        Assert.AreEqual(expected.PowerBombs, actual.PowerBombs);
        Assert.AreEqual(expected.ObstacleBitFlags, actual.ObstacleBitFlags);
        Assert.AreEqual(expected.DoorUnlockedFlags, actual.DoorUnlockedFlags);
    }
}
