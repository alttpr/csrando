namespace Randomizer.Graph;

/// <summary>
/// Representation of Players inventory for graph based traversal.
/// </summary>
public sealed class Inventory
{
    private readonly Dictionary<Item, int> _itemCount = new();
    private readonly Dictionary<World, float> _health = new();

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
    }

    public void AddItem(Item item, int count = 1)
    {
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
        return _itemCount.ContainsKey(item);
    }

    public bool Has(ItemCondition condition)
    {
        return HasAtLeast(condition.Item, condition.Count);
    }

    public bool HasAtLeast(Item item, int count)
    {
        return _itemCount.GetValueOrDefault(item, 0) >= count;
    }

    /// <summary>
    /// Get new Inventory with merge from another Inventory.
    /// </summary>
    /// <param name="inventory">Inventory to merge</param>
    public Inventory Merge(Inventory inventory)
    {
        var newInventory = new Inventory(this);

        foreach (var (item, count) in inventory._itemCount)
        {
            newInventory._itemCount[item] = newInventory._itemCount.GetValueOrDefault(item, 0) + count;
        }

        return newInventory;
    }

    /// <summary>
    /// Get the health value available based on items in this world.
    /// </summary>
    /// <param name="world">World for which we care about count</param>
    public float HeartCount(World world)
    {
        return _health.GetValueOrDefault(world, 0);
    }
}
