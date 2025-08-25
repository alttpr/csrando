using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Randomizer.Games.Alttp;
using Randomizer.Graph;

namespace RandomizerTests.Unit;

[TestClass]
public class DungeonKeySolverIntegrationTests
{
    /// <summary>
    /// Test that DungeonGraphConverter integration works without crashing
    /// </summary>
    [TestMethod]
    public void TestDungeonKeySolverIntegration_NoExceptions()
    {
        // This test verifies that the integration code compiles and can be called
        // without throwing exceptions, even with minimal setup
        
        var graph = new Randomizer.Graph.Graph();
        
        // Test the graph converter directly
        var dungeonGraph = DungeonGraphConverter.ExtractDungeonGraph(graph, "eastern", "KeyP1");
        Assert.IsNotNull(dungeonGraph);
        
        // Test the static solver method
        var emptyGraph = new DungeonGraph
        {
            Nodes = Array.Empty<DungeonNode>(),
            Edges = Array.Empty<DungeonEdge>()
        };
        
        var entrances = new[] { 1 };
        Func<string, bool> itemCheck = req => true;
        
        var safeLocations = DungeonKeySolverStatic.SafeItemLocationsForDungeon(
            emptyGraph, "test", entrances, itemCheck, 0);
        
        Assert.IsNotNull(safeLocations);
        Assert.AreEqual(0, safeLocations.Count);
    }
    
    /// <summary>
    /// Test that DungeonGraphConverter can handle an empty graph gracefully
    /// </summary>
    [TestMethod]
    public void TestDungeonGraphConverter_EmptyGraph()
    {
        var graph = new Randomizer.Graph.Graph();
        
        // This should return an empty dungeon graph without crashing
        var dungeonGraph = DungeonGraphConverter.ExtractDungeonGraph(graph, "eastern", "KeyP1");
        
        Assert.IsNotNull(dungeonGraph);
        Assert.AreEqual(0, dungeonGraph.Nodes.Count);
        Assert.AreEqual(0, dungeonGraph.Edges.Count);
    }
    
    /// <summary>
    /// Test DungeonKeySolverStatic with an empty graph
    /// </summary>
    [TestMethod]
    public void TestDungeonKeySolverStatic_EmptyGraph()
    {
        var emptyGraph = new DungeonGraph
        {
            Nodes = Array.Empty<DungeonNode>(),
            Edges = Array.Empty<DungeonEdge>()
        };
        
        var entrances = new[] { 1 };
        Func<string, bool> itemCheck = req => true;
        
        // Should return empty set for empty graph
        var safeLocations = DungeonKeySolverStatic.SafeItemLocationsForDungeon(
            emptyGraph, "test", entrances, itemCheck, 0);
        
        Assert.IsNotNull(safeLocations);
        Assert.AreEqual(0, safeLocations.Count);
    }
}