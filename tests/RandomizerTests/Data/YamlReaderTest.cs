namespace RandomizerTests.Data;

using AlttpRandomizer.Graph;

[TestClass]
public class YamlReaderTest
{
    [TestMethod]
    public void LoadEntrancesTest()
    {
        // TODO: Add more entrance types when they're done.
        var result = YamlReader.LoadEntrances("vanilla");
        Assert.IsTrue(result.Fixed.Count > 0);
    }

    [TestMethod]
    public void LoadEdgesTest()
    {
        foreach (var name in new string[] { "base", "inverted", "normal", "open", "standard" })
        {
            var result = YamlReader.LoadEdges(name);
            Assert.IsTrue(result.Count > 0);
        }
    }

    [TestMethod]
    public void LoadTechEdgesTest()
    {
        // TODO: Add more tech types when they're done.
        foreach (var name in new string[] { "fake_flippers" })
        {
            var result = YamlReader.LoadEdgesFromTech(name);
            Assert.IsTrue(result.Count > 0);
        }
    }

    [TestMethod]
    public void LoadVerticesTest()
    {
        var result = YamlReader.LoadVertices();
    }

    [TestMethod]
    public void LoadItemsTest()
    {
        var result = YamlReader.LoadItems();
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public void LoadEnemiesTest()
    {
        var result = YamlReader.LoadEnemies();
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public void LoadSpriteLocationsTest()
    {
        var result = YamlReader.LoadSpriteLocations();
        Assert.IsTrue(result.Count > 0);
    }
}
