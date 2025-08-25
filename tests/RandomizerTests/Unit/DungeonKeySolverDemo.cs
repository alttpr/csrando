using Randomizer.Graph;

namespace RandomizerTests.Unit;

/// <summary>
/// Demonstration of how the DungeonKeySolver would be integrated with reverse-fill Step 3
/// </summary>
public class DungeonKeySolverDemo
{
    /// <summary>
    /// Example of how the solver would be used in reverse-fill item placement
    /// </summary>
    public static void DemonstrateReverseFillIntegration()
    {
        // Example dungeon: Eastern Palace-like structure
        var nodes = new List<DungeonNode>
        {
            new() { Id = 100, DungeonId = "EasternPalace", IsLocation = false, StaticKeys = new() }, // Entrance
            new() { Id = 101, DungeonId = "EasternPalace", IsLocation = true, StaticKeys = new() },  // Cannonball Chest
            new() { Id = 102, DungeonId = "EasternPalace", IsLocation = true, StaticKeys = new() },  // Map Chest  
            new() { Id = 103, DungeonId = "EasternPalace", IsLocation = true, StaticKeys = new() },  // Compass Chest
            new() { Id = 104, DungeonId = "EasternPalace", IsLocation = true, StaticKeys = new() },  // Big Key Chest (behind lamp)
            new() { Id = 105, DungeonId = "EasternPalace", IsLocation = true, StaticKeys = new() },  // Big Chest (needs big key)
            new() { Id = 106, DungeonId = "EasternPalace", IsLocation = true, StaticKeys = new() },  // Boss (needs big key + bow)
        };

        var edges = new List<DungeonEdge>
        {
            // Free access from entrance
            new() { From = 100, To = 101, Req = "fixed", DungeonId = "EasternPalace" }, // Entrance -> Cannonball
            new() { From = 100, To = 102, Req = "fixed", DungeonId = "EasternPalace" }, // Entrance -> Map
            new() { From = 100, To = 103, Req = "fixed", DungeonId = "EasternPalace" }, // Entrance -> Compass
            
            // Lamp required for Big Key Chest
            new() { From = 100, To = 104, Req = "Lamp", DungeonId = "EasternPalace" },   // Entrance -> Big Key (needs Lamp)
            
            // Big Key required for Big Chest and Boss
            new() { From = 100, To = 105, Req = "BigKeyP1", DungeonId = "EasternPalace" }, // Entrance -> Big Chest (needs Big Key)
            new() { From = 100, To = 106, Req = "BigKeyP1", DungeonId = "EasternPalace" }, // Entrance -> Boss area
        };

        var graph = new DungeonGraph { Nodes = nodes, Edges = edges };

        // Create solver
        var solver = new DungeonKeySolver(graph, "EasternPalace");

        // Simulate different game states during reverse-fill

        // State 1: Early game - no lamp, no big key available
        Console.WriteLine("=== Early Game State ===");
        var itemCheck1 = (string req) => req switch
        {
            "Lamp" => false,
            "BigKeyP1" => false,
            _ => false
        };
        
        var safeLocations1 = solver.SafeItemLocations(itemCheck1, initialKeys: 0, entranceNodeIds: new[] { 100 });
        Console.WriteLine($"Safe locations without Lamp/BigKey: [{string.Join(", ", safeLocations1)}]");
        // Expected: 101, 102, 103 (Cannonball, Map, Compass)

        // State 2: Mid game - have lamp, no big key  
        Console.WriteLine("\n=== Mid Game State (have Lamp) ===");
        var itemCheck2 = (string req) => req switch
        {
            "Lamp" => true,
            "BigKeyP1" => false,
            _ => false
        };
        
        var safeLocations2 = solver.SafeItemLocations(itemCheck2, initialKeys: 0, entranceNodeIds: new[] { 100 });
        Console.WriteLine($"Safe locations with Lamp: [{string.Join(", ", safeLocations2)}]");
        // Expected: 101, 102, 103, 104 (+ Big Key Chest)

        // State 3: Late game - have lamp and big key
        Console.WriteLine("\n=== Late Game State (have Lamp + BigKey) ===");
        var itemCheck3 = (string req) => req switch
        {
            "Lamp" => true,
            "BigKeyP1" => true,
            _ => false
        };
        
        var safeLocations3 = solver.SafeItemLocations(itemCheck3, initialKeys: 0, entranceNodeIds: new[] { 100 });
        Console.WriteLine($"Safe locations with Lamp + BigKey: [{string.Join(", ", safeLocations3)}]");
        // Expected: 101, 102, 103, 104, 105 (+ Big Chest, but not Boss since no bow)

        // Demonstrate performance with precomputed solver
        Console.WriteLine("\n=== Performance Demo ===");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 1000; i++)
        {
            // Reuse same solver instance - this is the key performance benefit
            solver.SafeItemLocations(itemCheck3, initialKeys: 0, entranceNodeIds: new[] { 100 });
        }
        
        stopwatch.Stop();
        Console.WriteLine($"1000 queries took {stopwatch.ElapsedMilliseconds}ms (avg {stopwatch.ElapsedMilliseconds / 1000.0:F2}ms per query)");

        // Demonstrate static method for one-off usage
        Console.WriteLine("\n=== One-shot Usage ===");
        var oneShot = DungeonKeySolverStatic.SafeItemLocationsForDungeon(
            graph, "EasternPalace", new[] { 100 }, itemCheck1, initialKeys: 0);
        Console.WriteLine($"One-shot result: [{string.Join(", ", oneShot)}]");
    }
}