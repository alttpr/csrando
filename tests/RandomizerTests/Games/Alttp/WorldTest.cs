namespace RandomizerTests.Games.Alttp;

using Randomizer.Games.Alttp;
using Randomizer.Graph;

[TestClass]
public sealed class WorldTest
{
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
}
