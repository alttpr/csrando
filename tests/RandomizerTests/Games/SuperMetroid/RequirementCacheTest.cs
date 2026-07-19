namespace RandomizerTests.Games.SuperMetroid;

using Randomizer.Games;
using Randomizer.Games.SuperMetroid;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;

[TestClass]
public sealed class RequirementCacheTest
{
    [TestMethod]
    public void ModelCache_IsolatedBySearchPassEpoch()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        graph.SetVertexIds();

        var model = SmSearchModel.For(world, graph);
        var requirement = new Requirement.Single("Morph");
        var plan = model.GetRequirementPlan(
            requirement, world.RequirementHandler);
        int firstEpoch = model.BeginRequirementCacheEpoch();
        var firstResult = RequirementResult.Fail("Morph");
        model.SetRequirementResult(plan.Id, firstEpoch, firstResult);

        Assert.IsTrue(model.TryGetRequirementResult(
            plan.Id, firstEpoch, out var cached));
        Assert.AreSame(firstResult.Missing, cached.Missing);

        int secondEpoch = model.BeginRequirementCacheEpoch();
        Assert.IsFalse(model.TryGetRequirementResult(
            plan.Id, secondEpoch, out _));

        model.SetRequirementResult(plan.Id, firstEpoch, firstResult);
        Assert.IsFalse(model.TryGetRequirementResult(
            plan.Id, secondEpoch, out _),
            "A stale search pass must not populate the current cache epoch.");

        var secondResult = RequirementResult.Success(
            RequirementCost.ZeroCost);
        model.SetRequirementResult(plan.Id, secondEpoch, secondResult);
        Assert.IsTrue(model.TryGetRequirementResult(
            plan.Id, secondEpoch, out cached));
        Assert.IsTrue(cached.Met);
    }

    [TestMethod]
    public void StateCache_KeyedByMaskedBitsAndEpoch()
    {
        var (world, model) = BuildModel();
        var requirement = new Requirement.ObstaclesCleared(["A"]);
        var plan = model.GetRequirementPlan(
            requirement, world.RequirementHandler);
        Assert.IsTrue(plan.GraphStateDependent);
        Assert.IsFalse(plan.ResourceDependent);

        int epoch = model.BeginRequirementCacheEpoch();
        var cleared = RequirementResult.Success(RequirementCost.ZeroCost);
        model.SetStateRequirementResult(plan.Id, epoch, 1, 0, cleared);

        Assert.IsTrue(model.TryGetStateRequirementResult(
            plan.Id, epoch, 1, 0, out var cached));
        Assert.IsTrue(cached.Met);
        Assert.IsFalse(model.TryGetStateRequirementResult(
                plan.Id, epoch, 0, 0, out _),
            "States differing in a masked bit must not share a result.");

        int nextEpoch = model.BeginRequirementCacheEpoch();
        Assert.IsFalse(model.TryGetStateRequirementResult(
                plan.Id, nextEpoch, 1, 0, out _),
            "A new epoch must not observe results from the previous one.");
    }

    [TestMethod]
    public void PlanIds_SharedByValueEqualRequirements()
    {
        var (world, model) = BuildModel();
        var first = new Requirement.Single("h_canOpenGreenDoors");
        var second = new Requirement.Single("h_canOpenGreenDoors");
        Assert.AreNotSame(first, second);

        var firstPlan = model.GetRequirementPlan(
            first, world.RequirementHandler);
        var secondPlan = model.GetRequirementPlan(
            second, world.RequirementHandler);
        Assert.AreEqual(firstPlan.Id, secondPlan.Id,
            "Value-equal requirement instances must share one cache id.");

        var distinct = new Requirement.Single("h_canOpenYellowDoors");
        var distinctPlan = model.GetRequirementPlan(
            distinct, world.RequirementHandler);
        Assert.AreNotEqual(firstPlan.Id, distinctPlan.Id);
    }

    [TestMethod]
    public void StrategyPlans_CachedOnStratAndResetByClone()
    {
        var (world, model) = BuildModel();
        var strategy = new Strat
        {
            Name = "Test Strategy",
            Requires = new Requirement.Single("Morph"),
            ClearsObstacles = ["A"],
        };

        var plan = model.GetStrategyPlan(strategy, world.RequirementHandler);
        Assert.IsNotNull(strategy.CompiledPlan);
        Assert.AreEqual(plan, model.GetStrategyPlan(
            strategy, world.RequirementHandler));

        var clone = (Strat)strategy.Clone();
        Assert.IsNull(clone.CompiledPlan,
            "A clone may be mutated afterwards and must not inherit the plan.");
    }

    private static (World World, SmSearchModel Model) BuildModel()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        graph.SetVertexIds();
        return (world, SmSearchModel.For(world, graph));
    }
}
