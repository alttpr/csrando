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
    private readonly Dictionary<ItemSetName, int> _emptyCounts = new();

    public IReadOnlyList<Vertex> this[ItemSetName itemSet] => _setLocations.GetValueOrDefault(itemSet, EmptyList);
    public void Add(Vertex vertex, params ItemSetName[] itemSets)
    {
        foreach (var itemSet in itemSets)
        {
            _setLocations.TryAdd(itemSet, []);
            _setLocations[itemSet].Add(vertex);
            // Initialize empty counts during build
            if (vertex.Item == null)
            {
                _emptyCounts[itemSet] = _emptyCounts.GetValueOrDefault(itemSet, 0) + 1;
            }
        }
    }

    public int GetEmptyCount(ItemSetName itemSet) => _emptyCounts.GetValueOrDefault(itemSet, 0);

    public void NotifyPlaced(Vertex location)
    {
        if (location.Item == null)
            return; // only react to actual placements
        // Default set always tracked
        DecrementIfPresent(ItemSetName.DefaultSet);
        // Decrement for all explicit sets the location belongs to
        foreach (var s in location.ItemSet)
            DecrementIfPresent(s);

        void DecrementIfPresent(ItemSetName set)
        {
            if (_emptyCounts.TryGetValue(set, out int cnt) && cnt > 0)
                _emptyCounts[set] = cnt - 1;
        }
    }
}
