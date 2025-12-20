namespace Randomizer.Graph;

using Randomizer.Games;

public static class GameIds
{
    public const string Zelda3 = "alttp";
}

public abstract class World<TItem>(string gameId, int id, Graph graph, WorldConfig worldConfig) : IWorld
    where TItem : IItem
{
    public string GameId { get; } = gameId;
    public int Id { get; } = id;
    public Graph Graph { get; } = graph;
    public WorldConfig WorldConfig { get; } = worldConfig;

    // derived class is responsible for setting those; but more often than not they can't just pass it into the base ctor.
    public Vertex Start { get; protected init; } = null!;
    public Inventory StartingItems { get; protected init; } = null!;

    protected readonly Dictionary<string, TItem> _allItems = [];
    public ushort PlacedItemCount { get; set; }

    IItem IWorld.GetItem(string name) => GetItem(name);
    public TItem GetItem(string name)
    {
        if (_allItems.TryGetValue(name, out var matchingItem))
        {
            return matchingItem;
        }

        // allow made up items
        var item = Graph.RegisterItem(CreateItem(name, this));
        _allItems.Add(item.Name, item);

        return item;
    }
    protected abstract TItem CreateItem(string name, IWorld world);

    public TItem? GetItemOrNull(string? name)
    {
        if (name != null)
            return GetItem(name);
        return default;
    }

    IItem? IWorld.GetExistingItem(string name) => GetExistingItem(name);
    public TItem? GetExistingItem(string name)
    {
        if (_allItems.TryGetValue(name, out var item))
            return item;
        return default;
    }

    public IEnumerable<TItem> GetAllItems() => _allItems.Values;

    /// <summary>
    /// Get a vertex by name in this world.
    /// </summary>
    /// <param name="locationName">name to search for</param>
    public Vertex GetLocation(string locationName) => Graph.GetVertex($"{locationName}:{GameId}:{Id}");
    public bool HasLocation(string locationName) => Graph.HasVertex($"{locationName}:{GameId}:{Id}");

    /// <summary>Get all vertices in this world.</summary>
    /// <returns></returns>
    public IEnumerable<Vertex> GetLocations() => Graph.GetVertices().Where(vertex => vertex.World == this);
    /// <summary>Get all vertices of a given type in this world.</summary>
    /// <param name="type">type to search for</param>
    public IEnumerable<Vertex> GetLocationsOfType(VertexType type) => GetLocations().Where(vertex => vertex.Type == type);

    public void TrackPlacedItem(Vertex location)
    {
        if (ShouldTrack(location))
            location.World.PlacedItemCount++;
    }
    protected virtual bool ShouldTrack(Vertex location) => true;

    public virtual IEnumerable<Vertex> GetEmptyLocationsInSet(ISearcher searcher, IItem itemToPlace, ItemSetName itemSet, Dictionary<ItemSetName, int> setCounts)
        => searcher.GetEmptyLocationsInSet(itemSet, setCounts);

    public abstract bool IsWinnable(Vertex start, Inventory startingInventory);

    public abstract ISearcher GetSearcherForWorld(Graph graph, Vertex? start, Inventory inventory, SetLocations? setLocations = null);
}
