namespace Randomizer.Graph;

using System.Collections;
using System.Diagnostics;

/// <summary>
/// Representation of Players inventory for graph based traversal.
/// </summary>
public sealed class Inventory
{
    private BitArray _bits = new BitArray(400);
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
        if (other._bits != null)
        {
            _bits = (BitArray)other._bits.Clone();
        }
    }

    [ConditionalAttribute("DEBUG")]
    private static void CheckItemId(Item item)
    {
        if (item.Id < 0)
        {
            System.Console.WriteLine($"Item {item.Name} does not have an ID");
        }
    }

    public void AddItem(Item item, int count = 1)
    {
        if (item.Id >= _bits.Length)
        {
            // Increase the size by chunks of 4 bytes or 32 values
            _bits.Length = (item.Id / 31) * 32;
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
            if (newInventory._bits != null)
            {
                newInventory._bits.Set(item.Id, true);
            }
        }

        return newInventory;
    }
    public Inventory Clone() => new(this);

    /// <summary>
    /// Get the health value available based on items in this world.
    /// </summary>
    /// <param name="world">World for which we care about count</param>
    public float HeartCount(World world)
    {
        return _health.GetValueOrDefault(world, 0);
    }
}
