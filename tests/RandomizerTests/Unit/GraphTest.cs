namespace RandomizerTests.Unit;

using Randomizer.Graph;

[TestClass]
public sealed class GraphTest
{
    [TestMethod]
    public void TestAllDoors()
    {
        var randomizer = new Randomizer([new WorldConfig()], 1337);
        var world = randomizer.Worlds[0];

        Assert.AreEqual(4, randomizer.Graph.Doors[world.GetItem("KeyA1")].Count);
        Assert.AreEqual(8, randomizer.Graph.Doors[world.GetItem("KeyA2")].Count);
        Assert.AreEqual(4, randomizer.Graph.Doors[world.GetItem("KeyH2")].Count);
        Assert.AreEqual(2, randomizer.Graph.Doors[world.GetItem("KeyP1")].Count);
        Assert.AreEqual(4, randomizer.Graph.Doors[world.GetItem("KeyP2")].Count);
        Assert.AreEqual(1, randomizer.Graph.Doors[world.GetItem("KeyP3")].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[world.GetItem("KeyD1")].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[world.GetItem("KeyD2")].Count);
        Assert.AreEqual(5, randomizer.Graph.Doors[world.GetItem("KeyD3")].Count);
        Assert.AreEqual(3, randomizer.Graph.Doors[world.GetItem("KeyD4")].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[world.GetItem("KeyD5")].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[world.GetItem("KeyD6")].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[world.GetItem("KeyD7")].Count);
    }
}
