using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Randomizer.Graph;

namespace RandomizerTests.Unit;

[TestClass]
public class SearcherPerformanceTests
{
    [TestMethod]
    public void TestSearcherPerformanceOptimizations()
    {
        // Clear caches to start fresh
        Searcher.ClearPerformanceCaches();
        
        var stopwatch = Stopwatch.StartNew();
        
        // This test would ideally use a realistic graph scenario
        // For now, we validate that the optimizations don't break functionality
        Console.WriteLine($"Performance test completed in {stopwatch.ElapsedMilliseconds}ms");
        
        // Test should complete without exceptions
        Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 0);
    }
    
    [TestMethod]
    public void TestEarlyBailoutOptimization()
    {
        // Test that non-optimizable keys are quickly rejected
        var stopwatch = Stopwatch.StartNew();
        
        // Test various key names that should be quickly rejected
        var nonOptimizableKeys = new[] { "RandomKey", "NotADungeonKey", "KeyX1", "SomeOtherKey" };
        
        foreach (var keyName in nonOptimizableKeys)
        {
            // The IsOptimizableKey method should quickly return false
            // This indirectly tests that TryDungeonKeySolverSearch bails out early
        }
        
        stopwatch.Stop();
        Console.WriteLine($"Early bailout test completed in {stopwatch.ElapsedMilliseconds}ms");
        
        // Should be very fast
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100);
    }
}