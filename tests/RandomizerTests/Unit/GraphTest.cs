namespace RandomizerTests.Unit;

using Randomizer.Graph;

[TestClass]
public sealed class GraphTest
{
    [TestMethod]
    public void TestAllDoors()
    {
        var randomizer = new Randomizer([new WorldConfig()], 1337);

        Assert.AreEqual(4, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyA1", 0)].Count);
        Assert.AreEqual(8, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyA2", 0)].Count);
        Assert.AreEqual(4, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyH2", 0)].Count);
        Assert.AreEqual(2, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyP1", 0)].Count);
        Assert.AreEqual(4, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyP2", 0)].Count);
        Assert.AreEqual(1, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyP3", 0)].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyD1", 0)].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyD2", 0)].Count);
        Assert.AreEqual(5, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyD3", 0)].Count);
        Assert.AreEqual(3, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyD4", 0)].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyD5", 0)].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyD6", 0)].Count);
        Assert.AreEqual(6, randomizer.Graph.Doors[randomizer.GetItemForWorld("KeyD7", 0)].Count);
    }
}
