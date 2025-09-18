namespace Randomizer.Games.Alttp;

using Randomizer.Graph;

/// <summary>
/// @todo what item type is the hammer?
/// </summary>
public enum ItemType
{
    Medallion,
    Meta,
    SmallKey,
    BigKey,
    Map,
    Compass,
}

public sealed class Item : Randomizer.Graph.Item
{
    public ItemType Type { get; }
    public byte[]? Bytes { get; }

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
        Bytes = yamlItem?.Bytes.ToArray();

        if (Name.StartsWith("HeartContainer"))
            HealthValue = 1f;
        else if (Name.StartsWith("PieceOfHeart"))
            HealthValue = 0.25f;

        if (Name.StartsWith("Bottle"))
            LogicalItem = world.GetItem("LogicalBottle");
    }
}
