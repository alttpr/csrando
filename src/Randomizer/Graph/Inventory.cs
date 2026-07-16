namespace Randomizer.Graph;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// Representation of Players inventory for graph based traversal.
/// Item counts are stored in a flat array indexed by <see cref="IItem.Id"/> (ids are dense,
/// assigned by <see cref="Graph.RegisterItem"/>), so lookups are array reads and Clone — which
/// the key-door search performs for every explored state — is a flat array copy.
/// </summary>
public sealed class Inventory
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private const int InitialCapacity = 400;

    private int[] _counts;
    // Item instance for each held id, so the inventory can be enumerated (All/Merge).
    private IItem?[] _items;
    private readonly Dictionary<IWorld, float> _health = new();

    /// <summary>
    /// Bumped on every mutation; lets callers cheaply detect that an inventory is unchanged
    /// (the door search skips key types whose inputs haven't changed since their last run).
    /// </summary>
    internal int Version { get; private set; }

    public Inventory(params IItem[] items)
    {
        _counts = new int[InitialCapacity];
        _items = new IItem?[InitialCapacity];
        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    private Inventory(Inventory other)
    {
        // Faster than array.Clone() (MemberwiseClone); Clone runs for every state the
        // key-door search explores.
        _counts = GC.AllocateUninitializedArray<int>(other._counts.Length);
        Array.Copy(other._counts, _counts, _counts.Length);
        _items = new IItem?[other._items.Length];
        Array.Copy(other._items, _items, _items.Length);
        _health = new(other._health);
        Version = other.Version;
    }

    [Conditional("DEBUG")]
    private static void CheckItemId(IItem item)
    {
        if (item.Id < 0)
            _logger.LogWarning("Item {Name} does not have an ID. Make sure to call Graph.RegisterItem before using it.", item.Name);
    }

    private void EnsureCapacity(int id)
    {
        if (id < _counts.Length)
            return;

        int newLength = Math.Max(id + 1, _counts.Length * 2);
        Array.Resize(ref _counts, newLength);
        Array.Resize(ref _items, newLength);
    }

    public void AddItem(IItem item, int count = 1)
    {
        CheckItemId(item);

        EnsureCapacity(item.Id);
        _items[item.Id] = item;

        float healthValue = item.HealthValue;
        if (healthValue > 0f)
        {
            _health[item.World] = _health.GetValueOrDefault(item.World, 0) + healthValue;
        }

        if (item.LogicalItem is { } logicalItem)
        {
            AddItem(logicalItem);
        }

        _counts[item.Id] += count;
        Version++;
    }

    public void RemoveItem(IItem item, int count = 1)
    {
        if ((uint)item.Id >= (uint)_counts.Length || _counts[item.Id] == 0)
        {
            throw new Exception("Trying to remove an item not in inventory.");
        }

        int previousCount = _counts[item.Id];
        _counts[item.Id] = previousCount > count ? previousCount - count : 0;
        Version++;
    }

    /// <summary>
    /// Determine how many of a particular item are in inventory.
    /// </summary>
    /// <param name="item">Item to check</param>
    public int GetCount(IItem item)
    {
        if (item.LogicalItem is { } logicalItem)
        {
            return GetCountById(logicalItem.Id);
        }

        return GetCountById(item.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetCountById(int id)
    {
        return (uint)id < (uint)_counts.Length ? _counts[id] : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(IItem item)
    {
        return GetCountById(item.Id) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(ItemCondition condition)
    {
        if (condition.IsUnconditional)
        {
            return true;
        }
        return HasAtLeast(condition.Item, condition.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAtLeast(IItem item, int count)
    {
        return GetCountById(item.Id) >= count;
    }

    internal IReadOnlyDictionary<IItem, int> All()
    {
        var result = new Dictionary<IItem, int>();
        for (int id = 0; id < _counts.Length; id++)
        {
            if (_counts[id] > 0)
                result[_items[id]!] = _counts[id];
        }
        return result;
    }

    /// <summary>
    /// Get new Inventory with merge from another Inventory.
    /// </summary>
    /// <param name="inventory">Inventory to merge</param>
    public Inventory Merge(Inventory inventory)
    {
        var newInventory = new Inventory(this);
        newInventory.EnsureCapacity(inventory._counts.Length - 1);

        for (int id = 0; id < inventory._counts.Length; id++)
        {
            if (inventory._counts[id] > 0)
            {
                newInventory._counts[id] += inventory._counts[id];
                newInventory._items[id] = inventory._items[id];
            }
        }

        return newInventory;
    }
    public Inventory Clone()
    {
        return new(this);
    }

    /// <summary>
    /// Get the health value available based on items in this world.
    /// </summary>
    /// <param name="world">World for which we care about count</param>
    public float Health(IWorld world)
    {
        return _health.GetValueOrDefault(world, 0);
    }
}
