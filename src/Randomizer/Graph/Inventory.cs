namespace AlttpRandomizer.Graph;

using System.Collections.Concurrent;

/**
 * Representation of Players inventory for graph based traversal.
 *
 * @immutable
 */
public sealed class Inventory
{
    private readonly ConcurrentDictionary<string, int> _itemCount = new();
    /** @var array<float> */
    private readonly ConcurrentDictionary<int, float> _health = new();

    /**
     * Create new Inventory instance.
     *
     * @param iterable<Item|string> items items to add
     *
     * @return void
     */
    public Inventory(params object[] items)
    {
        foreach (object item in items)
        {
            AddItemByName(item is Item i ? i.Name : (string)item);
        }
    }
    private Inventory(Inventory other)
    {
        _itemCount = new(other._itemCount);
        _health = new(other._health);
    }

    /**
     * Add item to inventory.
     *
     * @param Item|string item item to add
     */
    public Inventory AddItem(object item)
    {
        var newInventory = new Inventory(this);
        newInventory.AddItemByName(item is Item i ? i.Name : (string)item);

        return newInventory;
    }

    /**
     * Add an item to this by name.
     *
     * @param string item_name name of item
     */
    private void AddItemByName(string item_name)
    {
        int newCount = _itemCount.AddOrUpdate(item_name, 1, (_, v) => v + 1);

        if (item_name.StartsWith("HeartContainer"))
        {
            string[] parts = item_name.Split(':');
            if (!int.TryParse(parts.Skip(1).FirstOrDefault(), out int world_id))
                world_id = 0;
            _health.AddOrUpdate(world_id, 1, (_, v) => v + 1);
        }
        else if (item_name.StartsWith("PieceOfHeart"))
        {
            string[] parts = item_name.Split(':');
            if (!int.TryParse(parts.Skip(1).FirstOrDefault(), out int world_id))
                world_id = 0;
            _health.AddOrUpdate(world_id, 0.25f, (_, v) => v + 0.25f);
        }
        else if (item_name.StartsWith("Bottle"))
        {
            string[] parts = item_name.Split(':');
            if (!int.TryParse(parts.Skip(1).FirstOrDefault(), out int world_id))
                world_id = 0;
            AddItemByName($"LogicalBottle:{world_id}");
        }

        if (newCount > 1)
        {
            _itemCount[item_name + "|" + newCount] = 1;
        }
    }

    /**
     * Determine how many of a particular item are in inventory.
     * 
     * @param string key item name to search for
     */
    public int GetCount(string key)
    {
        if (key.StartsWith("Bottle"))
        {
            string[] itemParts = key.Split(':');
            if (!int.TryParse(itemParts.Skip(1).FirstOrDefault(), out int world_id))
                world_id = 0;
            key = $"LogicalBottle:{world_id}";
        }

        return _itemCount.GetValueOrDefault(key, 0);
    }

    /**
     * Verify if item is in inventory.
     *
     * @param string item_name
     */
    public bool Has(string itemName)
    {
        return _itemCount.GetValueOrDefault(itemName, 0) > 0;
    }

    /**
     * Get new Inventory with merge from another Inventory.
     *
     * @param self inventory Inventory to merge
     */
    public Inventory Merge(Inventory inventory)
    {
        var newInventory = new Inventory(this);

        foreach (var (key, count) in inventory._itemCount)
        {
            if (key.Contains('|'))
            {
                continue;
            }

            int newCount = newInventory._itemCount.AddOrUpdate(key, count, (_, v) => v + count);

            for (int i = 2; i <= newCount; ++i)
            {
                newInventory._itemCount[key + "|" + i] = 1;
            }
        }

        return newInventory;
    }

    /**
     * Get the health value available based on items in this world.
     * 
     * @param int world_id world id for which we care about count
     */
    public float HeartCount(int worldId)
    {
        return _health.GetValueOrDefault(worldId, 0);
    }
}
