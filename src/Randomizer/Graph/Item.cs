namespace Randomizer.Graph;

using System.Diagnostics;

public enum ItemType
{
    Medallion,
    Meta,
    SmallKey,
    BigKey,
    Map,
    Compass,
}

[DebuggerDisplay("{Name}|{World.Id}")]
public sealed class Item
{
    public string Name { get; }
    public World World { get; }
    public ItemType Type { get; }

    /// <summary>
    /// Create a new Item.
    /// </summary>
    /// 
    /// <param name="name">Unique name of item</param>
    /// <param name="world">World this item is in</param>
    public Item(string name, World world)
    {
        Name = name;
        World = world;

        var yamlItems = YamlReader.LoadItems();
        string typeString = yamlItems.GetValueOrDefault(name)?.Type ?? "Meta";
        if (!Enum.TryParse<ItemType>(typeString, out var itemType))
        {
            itemType = ItemType.Meta;
        }
        Type = itemType;
    }
}
