namespace Randomizer.Graph;

public interface IWorld
{
    /// <summary>Get an <see cref="Item"/> that exists in this world, or create a meta-item for it.</summary>
    Item GetItem(string name);
    /// <summary>Get an <see cref="Item"/> if it exists, or <c>null</c> if no item with <paramref name="name"/> is known to this world.</summary>
    Item? GetExistingItem(string name);

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
    IEnumerable<Vertex> GetLocationsOfType(VertexType type);
    bool HasLocation(string locationName);

    WorldConfig Config { get; }
    Graph Graph { get; }
    int Id { get; }
    ushort PlacedItemCount { get; set; }
    Inventory StartingItems { get; }
}
