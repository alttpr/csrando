global using PooledItem = (Randomizer.Graph.ItemSetName Set, int Weight, Randomizer.Graph.IItem Item);

namespace Randomizer.Graph;

public interface IItemPooler
{
    /// <summary>Get list of all items in their weighted sets.</summary>
    PooledItem[] Pool { get; }
    /// <summary>Get a list possible locations, keyed by item set.</summary>
    SetLocations SetLocations { get; }
}

public sealed class SetLocations
{
    private readonly Dictionary<ItemSetName, List<Vertex>> _setLocations = new() { { ItemSetName.DefaultSet, new() } };
    private static readonly List<Vertex> EmptyList = [];

    public IReadOnlyList<Vertex> this[ItemSetName itemSet] => _setLocations.GetValueOrDefault(itemSet, EmptyList);
    public void Add(Vertex vertex, params ItemSetName[] itemSets)
    {
        foreach (var itemSet in itemSets)
        {
            _setLocations.TryAdd(itemSet, []);
            _setLocations[itemSet].Add(vertex);
        }
    }
    
    public Dictionary<ItemSetName, List<Vertex>> All() => _setLocations;
}
