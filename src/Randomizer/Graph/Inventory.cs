namespace Randomizer.Graph;

using System.Collections;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

/// <summary>
/// Representation of Players inventory for graph based traversal.
/// </summary>
public sealed class Inventory
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private readonly BitArray _bits = new(400);
    private readonly Dictionary<Item, int> _itemCount = new();
    private readonly Dictionary<IWorld, float> _health = new();

    public Inventory(params Item[] items)
    {
        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    private Inventory(Inventory other)
    {
        _itemCount = new(other._itemCount);
        _health = new(other._health);
        if (other._bits != null)
        {
            _bits = (BitArray)other._bits.Clone();
        }
    }

    [Conditional("DEBUG")]
    private static void CheckItemId(Item item)
    {
        if (item.Id < 0)
            _logger.LogWarning("Item {Name} does not have an ID. Make sure to call Graph.RegisterItem before using it.", item.Name);
    }

    public void AddItem(Item item, int count = 1)
    {
        CheckItemId(item);

        if (item.Id >= _bits.Length)
        {
            _bits.Length = item.Id + 1;
        }
        _bits.Set(item.Id, true);

        if (item.Name.StartsWith("HeartContainer"))
        {
            _health[item.World] = _health.GetValueOrDefault(item.World, 0) + 1;
        }
        else if (item.Name.StartsWith("PieceOfHeart"))
        {
            _health[item.World] = _health.GetValueOrDefault(item.World, 0) + 0.25f;
        }
        else if (item.Name.StartsWith("Bottle"))
        {
            AddItem(item.World.GetItem("LogicalBottle"));
        }

        if (!_itemCount.TryAdd(item, count))
        {
            _itemCount[item] += count;
        }
    }

    public void RemoveItem(Item item, int count = 1)
    {
        if (!_bits.Get(item.Id))
        {
            throw new Exception("Trying to remove an item not in inventory.");
        }

        int previousCount = _itemCount[item];
        if (previousCount > count)
        {
            _itemCount[item] -= count;
        }
        else
        {
            _itemCount.Remove(item);
            _bits.Set(item.Id, false);
        }
    }

    /// <summary>
    /// Determine how many of a particular item are in inventory.
    /// </summary>
    /// <param name="item">Item to check</param>
    public int GetCount(Item item)
    {
        if (item.Name.StartsWith("Bottle"))
        {
            return _itemCount.GetValueOrDefault(item.World.GetItem("LogicalBottle"), 0);
        }

        return _itemCount.GetValueOrDefault(item, 0);
    }

    public bool Has(Item item)
    {
        if (_bits != null)
        {
            if (item.Id >= _bits.Length)
            {
                _bits.Length = item.Id + 1;
            }
            return _bits.Get(item.Id);
        }
        return _itemCount.ContainsKey(item);
    }

    public bool Has(ItemCondition condition)
    {
        if (condition.IsUnconditional)
        {
            return true;
        }
        return HasAtLeast(condition.Item, condition.Count);
    }

    public bool HasAtLeast(Item item, int count)
    {
        if (count == 1)
        {
            return Has(item);
        }
        return _itemCount.GetValueOrDefault(item, 0) >= count;
    }

    internal IReadOnlyDictionary<Item, int> All()
    {
        return _itemCount.AsReadOnly();
    }

    /// <summary>
    /// Get new Inventory with merge from another Inventory.
    /// </summary>
    /// <param name="inventory">Inventory to merge</param>
    public Inventory Merge(Inventory inventory)
    {
        var newInventory = new Inventory(this);
        if (_bits != null && inventory._bits != null)
            newInventory._bits!.Length = Math.Max(_bits.Length, inventory._bits.Length);

        foreach (var (item, count) in inventory._itemCount)
        {
            newInventory._itemCount[item] = newInventory._itemCount.GetValueOrDefault(item, 0) + count;
            newInventory._bits?.Set(item.Id, true);
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
    public float HeartCount(IWorld world)
    {
        return _health.GetValueOrDefault(world, 0);
    }
}
