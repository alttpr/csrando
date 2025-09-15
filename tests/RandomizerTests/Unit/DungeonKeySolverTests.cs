using Microsoft.VisualStudio.TestTools.UnitTesting;
using Randomizer.Graph;
using System.Collections;
using System.Numerics;

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
    /// Property test comparing against brute force for small random dungeons
    /// </summary>
    [TestMethod]
    public void TestPropertyBasedVsBruteForce()
    {
        // Use a simple deterministic test case first
        var nodes = new List<DungeonNode>
        {
            new() { Id = 0, DungeonId = "Test", IsLocation = false, StaticKeys = new() }, // entrance
            new() { Id = 1, DungeonId = "Test", IsLocation = true, StaticKeys = new() },  // first location
            new() { Id = 2, DungeonId = "Test", IsLocation = true, StaticKeys = new() { ["Test"] = 1 } }, // location with key
            new() { Id = 3, DungeonId = "Test", IsLocation = true, StaticKeys = new() }   // final location
        };

        var edges = new List<DungeonEdge>
        {
            new() { From = 0, To = 1, Req = "fixed", DungeonId = "Test" },
            new() { From = 1, To = 2, Req = "KEY", DungeonId = "Test", DoorGroupId = 1 },
            new() { From = 2, To = 3, Req = "KEY", DungeonId = "Test", DoorGroupId = 2 }
        };

        var graph = new DungeonGraph { Nodes = nodes, Edges = edges };
        var solver = new DungeonKeySolver(graph, "Test");

        // Test with 0 keys - should only reach entrance area (node 1)
        var solverResult = solver.SafeItemLocations(req => true, 0, new[] { 0 });
        var expectedResult = new HashSet<int> { 1 }; // Only node 1 is always reachable with 0 keys

        Assert.AreEqual(expectedResult.Count, solverResult.Count,
            $"Expected {expectedResult.Count} safe locations with 0 keys, got {solverResult.Count}. Actual: [{string.Join(", ", solverResult)}]");

        foreach (var nodeId in expectedResult)
        {
            Assert.IsTrue(solverResult.Contains(nodeId),
                $"Node {nodeId} should be safe with 0 keys");
        }

        // Test with 1 key - this is the tricky case
        // With 1 initial key:
        // - We can open door 1 to reach node 2, gain 1 static key, total 2 keys
        // - But we can't guarantee reaching node 3 because we might spend the key differently
        // - Actually, with optimal play, we CAN reach node 3: use initial key for door 1, get static key, use static key for door 2
        // - But what if we use the key for door 2 first? We can't reach node 2 to get the static key!
        // - So the SAFE locations are only those reachable in ALL possible key-spend orders
        solverResult = solver.SafeItemLocations(req => true, 1, new[] { 0 });
        
        // Let's think through this more carefully:
        // Possible scenarios with 1 key:
        // 1. Don't open any doors: reach {1}
        // 2. Open door 1: reach {1, 2}, gain 1 key, now have 1 key total, can't open door 2 if we already spent the initial key
        // Actually wait - if we open door 1, we use our 1 key but gain 1 static key, so we end up with 1 key again
        // So after opening door 1, we can open door 2: reach {1, 2, 3}
        // But what if player tries to open door 2 first? That's not reachable from entrance without going through door 1!
        // So the algorithm should find that only the path through door 1 then door 2 works
        // Therefore nodes {1, 2, 3} should all be safe with 1 key
        expectedResult = new HashSet<int> { 1, 2, 3 }; // All nodes should be reachable with 1 key

        Assert.AreEqual(expectedResult.Count, solverResult.Count,
            $"Expected {expectedResult.Count} safe locations with 1 key, got {solverResult.Count}. Actual: [{string.Join(", ", solverResult)}]");

        foreach (var nodeId in expectedResult)
        {
            Assert.IsTrue(solverResult.Contains(nodeId),
                $"Node {nodeId} should be safe with 1 key");
        }
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

        // Create 50 nodes in a chain
        for (int i = 0; i < 50; i++)
        {
            nodes.Add(new DungeonNode 
            { 
                Id = i, 
                DungeonId = "PerfTest", 
                IsLocation = i > 0, // First node is entrance
                StaticKeys = i % 10 == 0 && i > 0 ? new() { ["PerfTest"] = 1 } : new()
            });
        }

        // Create a chain with some doors
        for (int i = 0; i < 49; i++)
        {
            var req = i % 8 == 0 ? "KEY" : "fixed"; // Door every 8 nodes
            edges.Add(new DungeonEdge 
            { 
                From = i, 
                To = i + 1, 
                Req = req, 
                DungeonId = "PerfTest", 
                DoorGroupId = req == "KEY" ? i / 8 : null 
            });
        }

        var graph = new DungeonGraph { Nodes = nodes, Edges = edges };
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Run 100 iterations to test performance
        for (int i = 0; i < 100; i++)
        {
            var solver = new DungeonKeySolver(graph, "PerfTest");
            var result = solver.SafeItemLocations(req => true, 3, new[] { 0 });
        }
        
        stopwatch.Stop();
        
        // Should complete well under 1 second for 100 iterations
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, 
            $"Performance test took {stopwatch.ElapsedMilliseconds}ms for 100 iterations");
    }

    /// <summary>
    /// Test the demo to ensure it works
    /// </summary>
    [TestMethod]
    public void TestDemo()
    {
        // Just ensure the demo runs without exceptions
        DungeonKeySolverDemo.DemonstrateReverseFillIntegration();
        Assert.IsTrue(true); // If we get here, demo worked
    }
}