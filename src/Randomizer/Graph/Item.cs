namespace AlttpRandomizer.Graph;

/**
 * An Item is any collectable thing in game.
 */
public sealed class Item
{
    public byte[] Bytes { get; }
    public string RawName { get; }
    public string Name { get; }
    public string NiceName { get; }
    public string I18NName { get; }
    public int WorldId { get; }
    public bool Meta { get; private set; }

    private static readonly Dictionary<int, Dictionary<string, Item>> _items = new();
    private static Dictionary<string, byte[]> _rawItems = null!;


    /**
     * Get the Item by name
     *
     * @param string name Name of Item
     * @param int world_id World item belongs to
     */
    public static Item Get(string name, int worldId)
    {
        var itemsForWorld = All(worldId);
        string world_name = name + ":" + worldId;

        if (itemsForWorld.TryGetValue(world_name, out var matchingItem))
        {
            return matchingItem;
        }

        // allow made up items
        var item = new Item(name, new byte[0], worldId);
        item.Meta = true;
        itemsForWorld.Add(item.Name, item);

        return item;
    }

    /**
     * Get the all known Items.
     *
     * @param int world_id World item belongs to
     */
    public static Dictionary<string, Item> All(int worldId)
    {
        if (_items.TryGetValue(worldId, out var itemsForWorld))
        {
            return itemsForWorld;
        }

        _items.Add(worldId, itemsForWorld = new());

        _rawItems ??= YamlReader.LoadItems();

        foreach (var (item_name, item_data) in _rawItems)
        {
            itemsForWorld.Add($"{item_name}:{worldId}", new Item(item_name, item_data/*["bytes"]*/, worldId));
        }


        return All(worldId);
    }

    /**
     * Create a new Item.
     *
     * @param string name Unique name of item
     * @param int[]|null[] bytes data to write to Location addresses
     * @param int world_id world for which the item belongs
     *
     * @return void
     */
    public Item(string name, byte[] bytes, int worldId)
    {
        RawName = name;
        Name = name + ":" + worldId;
        I18NName = "item." + name;
        string? formatted = __(I18NName);
        NiceName = formatted ?? "";
        Bytes = bytes;
        WorldId = worldId;
    }
    private static string? __(string name)
    {
        return null; // TODO: this should call localization and return the translated name
    }

    /**
     * serialized version of Item.
     *
     * @return string
     */
    public override string ToString()
    {
        return Name;
    }
}
