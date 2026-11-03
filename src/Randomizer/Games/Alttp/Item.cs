namespace Randomizer.Games.Alttp;

using Randomizer.Graph;

/// <summary>
/// TODO: what item type is the hammer?
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
    public ushort Price { get; }
    public bool IsProgression { get; }

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
        Tier = ItemTiers.ParseDeclaration(yamlItem?.Tier);
        Price = yamlItem?.Price ?? 999;
        IsProgression = yamlItem?.IsProgression ?? false;

        if (Name.StartsWith("HeartContainer"))
            HealthValue = 1f;
        else if (Name.StartsWith("PieceOfHeart"))
            HealthValue = 0.25f;

        if (Name.StartsWith("Bottle"))
            LogicalItem = world.GetItem("LogicalBottle");
    }
}
