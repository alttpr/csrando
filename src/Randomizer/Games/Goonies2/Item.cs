namespace Randomizer.Games.Goonies2;

using Randomizer.Graph;

public enum ItemType
{
    Meta,
    SmallKey,
    Compass,
    Goonie,
}

public sealed class Item : IItem
{
    public int Id { get; set; } = -1;
    public string Name { get; }
    public IWorld World { get; }
    public ItemType Type { get; }
    public byte[]? Bytes { get; }
    public byte[]? Gfx { get; }

    /// <summary>
    /// Create a new Item.
    /// </summary>
    ///
    /// <param name="name">Unique name of item</param>
    /// <param name="world">World this item is in</param>
    public Item(string name, IWorld world)
    {
        Name = name;
        World = world;

        var yamlItems = YamlReader.LoadItems();
        var yamlItem = yamlItems.GetValueOrDefault(name);
        string typeString = yamlItem?.Type ?? "Meta";
        if (!Enum.TryParse<ItemType>(typeString, out var itemType))
            itemType = ItemType.Meta;
        Type = itemType;
        Bytes = yamlItem?.Bytes.ToArray();
    }

    public override string ToString() => $"{Name}:{World.Id}";
}
