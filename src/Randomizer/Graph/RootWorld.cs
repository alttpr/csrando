namespace Randomizer.Graph;

/// <summary>
/// This is an internal-use world that acts as host for <see cref="Graph"/> nodes that do not belong to a player world.
/// </summary>
internal sealed class RootWorld(Graph graph) : IWorld
{
    private static readonly Exception _doNotUseThis = new NotSupportedException("This world is not for players.");
    WorldConfig IWorld.WorldConfig => throw _doNotUseThis;
    Graph IWorld.Graph { get; } = graph;
    int IWorld.Id { get; } = -1;
    ushort IWorld.PlacedItemCount { get; set; }
    Inventory IWorld.StartingItems => throw _doNotUseThis;

    IItem IWorld.GetItem(string name) => throw _doNotUseThis;
    IItem? IWorld.GetExistingItem(string name) => throw _doNotUseThis;
    Vertex IWorld.GetLocation(string locationName) => throw _doNotUseThis;
    IEnumerable<Vertex> IWorld.GetLocations() => throw _doNotUseThis;
    bool IWorld.HasLocation(string locationName) => throw _doNotUseThis;
    public IEnumerable<Vertex> GetEmptyLocationsInSet(Searcher searcher, IItem itemToPlace, ItemSetName itemSet, Dictionary<ItemSetName, int> setCounts) => throw _doNotUseThis;
    public void TrackPlacedItem(Vertex location) => throw _doNotUseThis;
    public bool IsWinnable(Vertex start, Inventory startingInventory) => false;
}

internal sealed class RootVertex : Vertex { }
