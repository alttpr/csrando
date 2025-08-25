using Microsoft.VisualStudio.TestTools.UnitTesting;
using Randomizer.Graph;
using System.Collections;

namespace RandomizerTests.Unit;

[TestClass]
public class DungeonKeySolverTests
{
    /// <summary>
    /// Create a mini test graph as described in the spec
    /// </summary>
    private static DungeonGraph CreateMiniGraph(bool hasStaticKey = false)
    {
        var nodes = new List<DungeonNode>
        {
            new() { Id = 0, DungeonId = "TestDungeon", IsLocation = false, StaticKeys = new() },
            new() { Id = 1, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = hasStaticKey ? new() { ["TestDungeon"] = 1 } : new() },
            new() { Id = 2, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() },
            new() { Id = 3, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() }
        };

        var edges = new List<DungeonEdge>
        {
            // Entrance to first door
            new() { From = 0, To = 1, Req = "fixed", DungeonId = "TestDungeon" },
            // First door (requires key)
            new() { From = 1, To = 2, Req = "KEY", DungeonId = "TestDungeon", DoorGroupId = 1 },
            // Second door (requires Hammer)
            new() { From = 2, To = 3, Req = "Hammer", DungeonId = "TestDungeon" }
        };

        return new DungeonGraph { Nodes = nodes, Edges = edges };
    }

    [TestMethod]
    public void TestMiniGraph_NoHammer_NoKeys()
    {
        var graph = CreateMiniGraph();
        var solver = new DungeonKeySolver(graph, "TestDungeon");
        
        var result = solver.SafeItemLocations(req => false, 0, new[] { 0 });
        
        // Only entrance area should be safe without keys or hammer
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.Contains(1)); // Node 1 is reachable without keys
    }

    [TestMethod]
    public void TestMiniGraph_NoHammer_OneKey()
    {
        var graph = CreateMiniGraph();
        var solver = new DungeonKeySolver(graph, "TestDungeon");
        
        var result = solver.SafeItemLocations(req => false, 1, new[] { 0 });
        
        // With one key, we can reach node 2 but not 3 (needs Hammer)
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Contains(1));
        Assert.IsTrue(result.Contains(2));
    }

    [TestMethod]
    public void TestMiniGraph_WithHammer_OneKey()
    {
        var graph = CreateMiniGraph();
        var solver = new DungeonKeySolver(graph, "TestDungeon");
        
        var result = solver.SafeItemLocations(req => req == "Hammer", 1, new[] { 0 });
        
        // With hammer and one key, all nodes should be reachable
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Contains(1));
        Assert.IsTrue(result.Contains(2));
        Assert.IsTrue(result.Contains(3));
    }

    [TestMethod]
    public void TestMiniGraph_WithStaticKey()
    {
        var graph = CreateMiniGraph(hasStaticKey: true);
        var solver = new DungeonKeySolver(graph, "TestDungeon");
        
        var result = solver.SafeItemLocations(req => false, 0, new[] { 0 });
        
        // Static key behind first door should enable second door
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Contains(1));
        Assert.IsTrue(result.Contains(2));
    }

    [TestMethod]
    public void TestBidirectionalDoor()
    {
        var nodes = new List<DungeonNode>
        {
            new() { Id = 0, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() },
            new() { Id = 1, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() }
        };

        var edges = new List<DungeonEdge>
        {
            // Bidirectional door - same group ID
            new() { From = 0, To = 1, Req = "KEY", DungeonId = "TestDungeon", DoorGroupId = 1 },
            new() { From = 1, To = 0, Req = "KEY", DungeonId = "TestDungeon", DoorGroupId = 1 }
        };

        var graph = new DungeonGraph { Nodes = nodes, Edges = edges };
        var solver = new DungeonKeySolver(graph, "TestDungeon");
        
        var result = solver.SafeItemLocations(req => true, 1, new[] { 0 });
        
        // With one key, both sides should be accessible (door opens both ways)
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Contains(0));
        Assert.IsTrue(result.Contains(1));
    }

    [TestMethod]
    public void TestChainOfDoors()
    {
        var nodes = new List<DungeonNode>
        {
            new() { Id = 0, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() },
            new() { Id = 1, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() { ["TestDungeon"] = 1 } },
            new() { Id = 2, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() },
            new() { Id = 3, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() }
        };

        var edges = new List<DungeonEdge>
        {
            new() { From = 0, To = 1, Req = "KEY", DungeonId = "TestDungeon", DoorGroupId = 1 },
            new() { From = 1, To = 2, Req = "KEY", DungeonId = "TestDungeon", DoorGroupId = 2 },
            new() { From = 2, To = 3, Req = "KEY", DungeonId = "TestDungeon", DoorGroupId = 3 }
        };

        var graph = new DungeonGraph { Nodes = nodes, Edges = edges };
        var solver = new DungeonKeySolver(graph, "TestDungeon");
        
        // With one key, we can open first door, get static key, then open second door
        var result = solver.SafeItemLocations(req => true, 1, new[] { 0 });
        
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Contains(0));
        Assert.IsTrue(result.Contains(1));
        Assert.IsTrue(result.Contains(2));
        // Node 3 should not be safe - requires 2 keys but we only get 1 static + 1 initial
    }

    [TestMethod]
    public void TestMultipleEntrances()
    {
        var nodes = new List<DungeonNode>
        {
            new() { Id = 0, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() },
            new() { Id = 1, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() },
            new() { Id = 2, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() }
        };

        var edges = new List<DungeonEdge>
        {
            new() { From = 0, To = 2, Req = "KEY", DungeonId = "TestDungeon", DoorGroupId = 1 },
            new() { From = 1, To = 2, Req = "fixed", DungeonId = "TestDungeon" }
        };

        var graph = new DungeonGraph { Nodes = nodes, Edges = edges };
        var solver = new DungeonKeySolver(graph, "TestDungeon");
        
        // Multiple entrances - union of reachable areas
        var result = solver.SafeItemLocations(req => true, 0, new[] { 0, 1 });
        
        Assert.AreEqual(3, result.Count); // All nodes reachable via entrance 1
    }

    [TestMethod]
    public void TestEmptyResult()
    {
        var nodes = new List<DungeonNode>
        {
            new() { Id = 0, DungeonId = "TestDungeon", IsLocation = true, StaticKeys = new() }
        };

        var edges = new List<DungeonEdge>();

        var graph = new DungeonGraph { Nodes = nodes, Edges = edges };
        var solver = new DungeonKeySolver(graph, "TestDungeon");
        
        var result = solver.SafeItemLocations(req => true, 0, new int[0]);
        
        Assert.AreEqual(0, result.Count); // No entrances = no reachable nodes
    }

    [TestMethod]
    public void TestStaticSolverMethod()
    {
        var graph = CreateMiniGraph();
        
        var result = DungeonKeySolverStatic.SafeItemLocationsForDungeon(
            graph, "TestDungeon", new[] { 0 }, req => req == "Hammer", 1);
        
        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void TestSafeComponents()
    {
        var graph = CreateMiniGraph();
        var solver = new DungeonKeySolver(graph, "TestDungeon");
        
        var safeComps = solver.SafeComponents(req => false, 1, new[] { 0 });
        
        // Should return BitArray indicating which components are safe
        Assert.IsNotNull(safeComps);
        Assert.IsTrue(safeComps.Length > 0);
    }

    /// <summary>
    /// Performance test with a larger synthetic graph
    /// </summary>
    [TestMethod]
    public void TestPerformanceWithLargerGraph()
    {
        // Create a synthetic dungeon with multiple doors and components
        var nodes = new List<DungeonNode>();
        var edges = new List<DungeonEdge>();

        // Create 100 nodes
        for (int i = 0; i < 100; i++)
        {
            nodes.Add(new DungeonNode 
            { 
                Id = i, 
                DungeonId = "PerfTest", 
                IsLocation = true, 
                StaticKeys = i % 10 == 0 ? new() { ["PerfTest"] = 1 } : new()
            });
        }

        // Create a chain of doors
        for (int i = 0; i < 99; i++)
        {
            var req = i % 5 == 0 ? "KEY" : "fixed";
            edges.Add(new DungeonEdge 
            { 
                From = i, 
                To = i + 1, 
                Req = req, 
                DungeonId = "PerfTest", 
                DoorGroupId = req == "KEY" ? i : null 
            });
        }

        var graph = new DungeonGraph { Nodes = nodes, Edges = edges };
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Run 100 iterations to test performance
        for (int i = 0; i < 100; i++)
        {
            var solver = new DungeonKeySolver(graph, "PerfTest");
            var result = solver.SafeItemLocations(req => true, 5, new[] { 0 });
        }
        
        stopwatch.Stop();
        
        // Should complete well under 1 second for 100 iterations
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, 
            $"Performance test took {stopwatch.ElapsedMilliseconds}ms for 100 iterations");
    }
}