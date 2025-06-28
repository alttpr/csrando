namespace Randomizer.Games.Zelda1;

using Randomizer.Graph;

public enum ItemType
{
    Meta,
    SmallKey,
}

public sealed class Item : Randomizer.Graph.Item
{
    public ItemType Type { get; }
    public byte? Byte { get; }

    /// <summary>
    /// Create a new Item.
    /// </summary>
    ///
    /// <param name="name">Unique name of item</param>
    /// <param name="world">World this item is in</param>
    public Item(string name, IWorld world)
        : base(name, world)
    {
        var yamlItems = YamlReader.LoadItems();
        var yamlItem = yamlItems.GetValueOrDefault(name);
        string typeString = yamlItem?.Type ?? "Meta";
        if (!Enum.TryParse<ItemType>(typeString, out var itemType))
            itemType = ItemType.Meta;
        Type = itemType;
        Byte = yamlItem?.Byte;
    }
}
