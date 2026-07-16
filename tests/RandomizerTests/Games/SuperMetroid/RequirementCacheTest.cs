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
}
