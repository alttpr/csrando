namespace RandomizerTests.Games.Alttp;

using Randomizer.Games;
using Randomizer.Games.Alttp;
using Randomizer.Graph;

[TestClass]
public sealed class WorldTest
{
    [TestMethod]
    public void Vanilla_ItemPoolMatchesEmptyLocations()
    {
        var world = new World(0,
            new WorldConfig { Alttp = new Config() }, new(), new(1337));
        var pooler = new ItemPooler([world], new PRNG(1337));
        int emptyLocations = pooler.SetLocations[ItemSetName.DefaultSet]
            .Count(location => location.Item == null);

        Assert.AreEqual(emptyLocations, pooler.Pool.Length,
            $"Pool has {pooler.Pool.Length} items for {emptyLocations} empty locations.");
    }

    [TestMethod]
    public void TestAllDoors()
    {
        var world = new World(1, new WorldConfig { Alttp = new() }, new(), new(1337));
        var graph = world.Graph;

        Assert.AreEqual(4, graph.Doors[world.GetItem("KeyA1")].Count);
        Assert.AreEqual(8, graph.Doors[world.GetItem("KeyA2")].Count);
        Assert.AreEqual(4, graph.Doors[world.GetItem("KeyH2")].Count);
        Assert.AreEqual(2, graph.Doors[world.GetItem("KeyP1")].Count);
        Assert.AreEqual(4, graph.Doors[world.GetItem("KeyP2")].Count);
        Assert.AreEqual(1, graph.Doors[world.GetItem("KeyP3")].Count);
        Assert.AreEqual(6, graph.Doors[world.GetItem("KeyD1")].Count);
        Assert.AreEqual(6, graph.Doors[world.GetItem("KeyD2")].Count);
        Assert.AreEqual(5, graph.Doors[world.GetItem("KeyD3")].Count);
        Assert.AreEqual(3, graph.Doors[world.GetItem("KeyD4")].Count);
        Assert.AreEqual(6, graph.Doors[world.GetItem("KeyD5")].Count);
        Assert.AreEqual(6, graph.Doors[world.GetItem("KeyD6")].Count);
        Assert.AreEqual(6, graph.Doors[world.GetItem("KeyD7")].Count);
    }

    [TestMethod]
    [DataRow(StateOption.Open, "DarkDefeatGanon")]
    [DataRow(StateOption.Inverted, "DefeatGanon")]
    public void GanonDefeat_UsesTheRoomBunnyState(StateOption state, string requirement)
    {
        var world = new World(1,
            new WorldConfig { Alttp = new Config { State = state } }, new(), new(1337));
        var ganon = world.GetLocation("Ganon");
        var ganonEdges = world.Graph.GetVertices().SelectMany(vertex => vertex.Edges)
            .Where(edge => ReferenceEquals(edge.To, ganon)).ToList();

        Assert.IsTrue(ganonEdges.Count > 0);
        Assert.IsTrue(ganonEdges.All(edge => ReferenceEquals(
            world.GetItem(requirement), edge.Condition.Item)));

        var defeated = world.GetLocation("DefeatGanon");
        var darkDefeated = world.GetLocation("DarkDefeatGanon");
        var darkDefeatEdge = defeated.Edges.Single(edge => ReferenceEquals(edge.To, darkDefeated));
        Assert.AreSame(world.GetItem("MoonPearl"), darkDefeatEdge.Condition.Item);
    }
}
