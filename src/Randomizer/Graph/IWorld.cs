namespace Randomizer.Graph;

using Randomizer.Games;

public interface IWorld
{
    /// <summary>Get an <see cref="IItem"/> that exists in this world, or create a meta-item for it.</summary>
    IItem GetItem(string name);
    /// <summary>Get an <see cref="IItem"/> if it exists, or <c>null</c> if no item with <paramref name="name"/> is known to this world.</summary>
    IItem? GetExistingItem(string name);

    /// <summary>
    /// Get a vertex by name in this world.
    /// </summary>
    /// <param name="locationName">name to search for</param>
    Vertex GetLocation(string locationName);

    /// <summary>Get all vertices in this world.</summary>
    /// <returns></returns>
    IEnumerable<Vertex> GetLocations();

    /// <summary>Get all vertices of a given type in this world.</summary>
    /// <param name="type">type to search for</param>
    IEnumerable<Vertex> GetLocationsOfType(VertexType type) => GetLocations().Where(v => v.Type == type);
    bool HasLocation(string locationName);

    IEnumerable<Vertex> GetEmptyLocationsInSet(ISearcher searcher, IItem itemToPlace, ItemSetName itemSet, Dictionary<ItemSetName, int> setCounts);
    /// <summary>
    /// Tracks the item for <paramref name="location"/> as placed. This might affect a game's ability to determine how many items were placed in total.
    /// </summary>
    void TrackPlacedItem(Vertex location);
    /// <summary>
    /// Determines whether this world can be won when starting from <paramref name="start"/> with <paramref name="startingInventory"/>.
    /// </summary>
    /// <param name="start">Starting location of the player.</param>
    /// <param name="startingInventory">Starting items the player has innate access to.</param>
    bool IsWinnable(Vertex start, Inventory startingInventory);

    /// <summary>The logical items/events whose collection completes this world.</summary>
    IEnumerable<IItem> GetVictoryItems() => [];

    /// <summary>The vertex where a logical playthrough begins for this world.</summary>
    Vertex GetPlaythroughStart() => Start;

    /// <summary>
    /// Get a searcher for this specific world.
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="start"></param>
    /// <param name="inventory"></param>
    /// <param name="setLocations"></param>
    /// <returns></returns>
    ISearcher GetSearcherForWorld(Graph graph, Vertex? start, Inventory inventory, SetLocations? setLocations = null);

    WorldConfig WorldConfig { get; }
    Graph Graph { get; }
    int Id { get; }
    string GameId { get; }
    ushort PlacedItemCount { get; set; }
    Inventory StartingItems { get; }
    /// <summary>The entry point for this world, where everything begins.</summary>
    Vertex Start { get; }
}
