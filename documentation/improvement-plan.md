# CSRandomizer Improvement Plan

## Executive Summary
This document outlines a comprehensive plan to improve the CSRandomizer codebase with focus on performance optimization and enhanced game extensibility. The current architecture shows several areas where significant improvements can be made to support more games and achieve better performance.

## Performance Optimization Opportunities

### 1. Graph Search Algorithm Improvements

#### Current Issues:
- **Inefficient Search Pattern**: The `Searcher` class performs multiple passes through the graph with repeated inventory cloning
- **Memory Allocation**: Frequent creation of `VertexHashSet` clones and inventory copies
- **Recursive Door Search**: Complex recursive logic in `RecursiveDoorSearchInternal` can lead to exponential complexity

#### Proposed Solutions:
```csharp
// Implement incremental search with delta tracking
public class IncrementalSearcher
{
    private readonly Dictionary<Vertex, SearchDelta> _searchCache = new();
    private readonly Queue<SearchOperation> _pendingOperations = new();

    public SearchResult SearchIncremental(Inventory inventory, Vertex start)
    {
        // Track only changes from previous search
        // Cache reachability results
        // Batch operations for better performance
    }
}

// Optimize door search with memoization
private readonly Dictionary<(IItem, VertexHashSet), SearchResult> _doorSearchCache = new();
```

#### Expected Performance Gains:
- **50-70% reduction** in search time for complex graphs
- **30-40% reduction** in memory allocations
- **Elimination** of exponential complexity in door searches

### 2. Memory Management Optimization

#### Current Issues:
- **BitArray Resizing**: `VertexHashSet` and `Inventory` classes frequently resize BitArrays
- **Object Cloning**: Excessive use of `Clone()` methods throughout the codebase
- **LINQ Operations**: Multiple LINQ chains in hot paths

#### Proposed Solutions:
```csharp
// Pre-allocate BitArrays with known sizes
public class OptimizedVertexHashSet
{
    private readonly BitArray _bitArray;
    private readonly int _maxVertexId;

    public OptimizedVertexHashSet(Graph graph)
    {
        _maxVertexId = graph.GetVertices().Max(v => v.Id);
        _bitArray = new BitArray(_maxVertexId + 1);
    }
}

// Implement object pooling for frequently cloned objects
public class InventoryPool
{
    private readonly ConcurrentQueue<Inventory> _pool = new();

    public Inventory Rent() => _pool.TryDequeue(out var inventory) ? inventory : new Inventory();
    public void Return(Inventory inventory) => _pool.Enqueue(inventory);
}
```

#### Expected Performance Gains:
- **25-35% reduction** in memory allocations
- **Faster BitArray operations** with pre-allocated sizes
- **Reduced GC pressure** through object pooling

### 3. Item Placement Algorithm Optimization

#### Current Issues:
- **Linear Search**: `RandomAssumedFiller` performs linear searches through item lists
- **Repeated Searcher Creation**: New `Searcher` instances created for each item placement
- **Inefficient Location Finding**: `GetEmptyLocationsInSet` called multiple times

#### Proposed Solutions:
```csharp
// Implement spatial indexing for locations
public class LocationIndex
{
    private readonly Dictionary<ItemSetName, HashSet<Vertex>> _locationIndex = new();
    private readonly Dictionary<Vertex, HashSet<ItemSetName>> _reverseIndex = new();

    public IEnumerable<Vertex> GetLocationsForSet(ItemSetName set) => _locationIndex[set];
    public void UpdateIndex(Vertex location, ItemSetName set) { /* efficient updates */ }
}

// Batch item placement with dependency analysis
public class BatchItemPlacer
{
    public void PlaceItemsBatch(PooledItem[] items, LocationIndex index)
    {
        // Group items by dependencies
        // Place items in optimal order
        // Minimize searcher recreation
    }
}
```

#### Expected Performance Gains:
- **40-60% reduction** in item placement time
- **Elimination** of repeated searcher creation
- **Better cache locality** through spatial indexing

## Game Extensibility Improvements

### 1. Plugin Architecture

#### Current Issues:
- **Hard-coded Game Types**: `RandomizerFactory` uses switch expressions with hard-coded game types
- **Tight Coupling**: Game-specific logic embedded throughout the codebase
- **Limited Configuration**: `WorldConfig` requires explicit game type properties

#### Proposed Solutions:
```csharp
// Implement plugin system for games
public interface IGamePlugin
{
    string GameId { get; }
    Type ConfigType { get; }
    Type RandomizerType { get; }
    Type WorldType { get; }
    Type ItemPoolerType { get; }
}

// Dynamic plugin discovery
public class GamePluginManager
{
    private readonly Dictionary<string, IGamePlugin> _plugins = new();

    public void RegisterPlugin(IGamePlugin plugin) => _plugins[plugin.GameId] = plugin;
    public IGamePlugin GetPlugin(string gameId) => _plugins[gameId];

    public GameRandomizer CreateRandomizer(string gameId, WorldConfig config, PRNG prng)
    {
        var plugin = GetPlugin(gameId);
        return (GameRandomizer)Activator.CreateInstance(plugin.RandomizerType, config, prng);
    }
}
```

#### Benefits:
- **Easy addition** of new games without code changes
- **Runtime game discovery** and registration
- **Cleaner separation** of game-specific logic

### 2. Configuration System Enhancement

#### Current Issues:
- **Game-specific Properties**: Each game requires explicit configuration properties
- **Type Safety Issues**: Configuration validation happens at runtime
- **Limited Extensibility**: Adding new game types requires code changes

#### Proposed Solutions:
```csharp
// Generic configuration system
public class GameConfig<T> where T : class
{
    public string GameId { get; init; }
    public T Settings { get; init; }
    public Dictionary<string, object> Extensions { get; init; } = new();
}

// Dynamic configuration validation
public interface IConfigValidator
{
    ValidationResult Validate(object config);
}

public class ConfigValidatorRegistry
{
    private readonly Dictionary<Type, IConfigValidator> _validators = new();

    public void RegisterValidator<T>(IConfigValidator validator)
        => _validators[typeof(T)] = validator;
}
```

#### Benefits:
- **Type-safe configuration** for all games
- **Extensible settings** through dynamic properties
- **Runtime validation** with custom rules

### 3. Modular World System

#### Current Issues:
- **Abstract Base Classes**: Heavy inheritance hierarchy limits flexibility
- **Game-specific Implementations**: World logic tightly coupled to specific games
- **Limited Composition**: Difficult to mix and match world features

#### Proposed Solutions:
```csharp
// Component-based world system
public interface IWorldComponent
{
    string ComponentId { get; }
    void Initialize(World world);
}

public class World
{
    private readonly Dictionary<string, IWorldComponent> _components = new();

    public T GetComponent<T>() where T : class, IWorldComponent
        => _components.Values.OfType<T>().FirstOrDefault();

    public void AddComponent(IWorldComponent component)
    {
        _components[component.ComponentId] = component;
        component.Initialize(this);
    }
}

// Reusable world components
public class DungeonComponent : IWorldComponent
{
    public string ComponentId => "Dungeon";
    public List<Dungeon> Dungeons { get; } = new();
}

public class OverworldComponent : IWorldComponent
{
    public string ComponentId => "Overworld";
    public List<Region> Regions { get; } = new();
}
```

#### Benefits:
- **Composable world features** across different games
- **Easier testing** of individual components
- **Reusable logic** between similar game types

## Implementation Roadmap

### Phase 1: Performance Foundation (Weeks 1-4)
1. **Implement object pooling** for Inventory and VertexHashSet
2. **Optimize BitArray operations** with pre-allocation
3. **Add search result caching** in Searcher class
4. **Profile and benchmark** current performance bottlenecks

### Phase 2: Algorithm Optimization (Weeks 5-8)
1. **Refactor RandomAssumedFiller** with batch processing
2. **Implement incremental search** with delta tracking
3. **Optimize door search** with memoization
4. **Add spatial indexing** for location queries

### Phase 3: Plugin Architecture (Weeks 9-12)
1. **Design plugin interface** and registration system
2. **Refactor RandomizerFactory** to use plugins
3. **Create sample plugin** for a new game type
4. **Update configuration system** for dynamic game support

### Phase 4: Component System (Weeks 13-16)
1. **Implement component-based world system**
2. **Refactor existing games** to use components
3. **Create reusable component library**
4. **Document plugin development** guidelines

### Phase 5: Testing and Optimization (Weeks 17-20)
1. **Comprehensive performance testing**
2. **Memory usage optimization**
3. **Plugin system validation**
4. **Documentation and examples**

## Expected Outcomes

### Performance Improvements:
- **Overall speedup**: 2-3x faster randomization
- **Memory usage**: 40-50% reduction in allocations
- **Scalability**: Support for larger, more complex games

### Extensibility Improvements:
- **New game addition**: From weeks to hours
- **Plugin ecosystem**: Community-driven game support
- **Configuration flexibility**: Runtime game discovery and configuration

### Code Quality:
- **Maintainability**: Cleaner separation of concerns
- **Testability**: Easier unit testing of components
- **Documentation**: Better developer experience for plugin authors

## Risk Mitigation

### Technical Risks:
- **Breaking Changes**: Implement changes incrementally with feature flags
- **Performance Regression**: Comprehensive benchmarking at each phase
- **Plugin Compatibility**: Maintain backward compatibility during transition

### Timeline Risks:
- **Scope Creep**: Strict adherence to phase objectives
- **Integration Issues**: Early testing of plugin system with existing games
- **Performance Degradation**: Continuous monitoring and rollback capabilities

## Conclusion

This improvement plan addresses the core limitations of the current CSRandomizer architecture while maintaining its strengths. The phased approach ensures minimal disruption to existing functionality while delivering significant performance and extensibility improvements. The resulting system will be more maintainable, faster, and easier to extend with new games and features.
