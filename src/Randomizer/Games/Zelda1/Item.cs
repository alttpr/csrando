namespace Randomizer.Games.Zelda1;

using Randomizer.Graph;

public enum ItemType
{
    Meta,
    SmallKey,
}

public sealed class Item : IItem
{
    public int Id { get; set; } = -1;
    public string Name { get; }
    public IWorld World { get; }
    public ItemType Type { get; }
    public byte? Byte { get; }
    public float HealthValue { get; } = 0; // FIXME: is there health increase anywhere?
    public IItem? LogicalItem { get; } // FIXME: are there logic-relevant items that represent viable alternatives?

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
        Byte = yamlItem?.Byte;
    }

    public override string ToString() => $"{Name}:{World.GameId}:{World.Id}";
}
