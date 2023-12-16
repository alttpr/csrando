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

public sealed class Item
{
    public int Id { get; set; } = -1;
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

    public override string ToString() => $"{Name}:{World.Id}";
}
