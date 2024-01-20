namespace Randomizer.Graph;

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
    public byte[]? Bytes { get; }
    public string? PedestalHintText { get; }
    public string? PedestalCreditsText { get; }
    public string? EtherTabletHintText { get; }
    public string? BombosTabletHintText { get; }
    public string? UncleCreditsText { get; }
    public string? FluteCreditsText { get; }
    public string? KidCreditsText { get; }
    public string? WitchCreditsText { get; }
    public string? ZoraCreditsText { get; }

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
        var yamlItem = yamlItems.GetValueOrDefault(name);
        string typeString = yamlItem?.Type ?? "Meta";
        if (!Enum.TryParse<ItemType>(typeString, out var itemType))
            itemType = ItemType.Meta;
        Type = itemType;
        Bytes = yamlItem?.Bytes.ToArray();
        PedestalHintText = yamlItem?.PedestalHintText;
        EtherTabletHintText = yamlItem?.EtherTabletHintText;
        BombosTabletHintText = yamlItem?.BombosTabletHintText;
        PedestalCreditsText = yamlItem?.PedestalCreditsText;
        UncleCreditsText = yamlItem?.UncleCreditsText;
        FluteCreditsText = yamlItem?.FluteCreditsText;
        KidCreditsText = yamlItem?.KidCreditsText;
        WitchCreditsText = yamlItem?.WitchCreditsText;
        ZoraCreditsText  = yamlItem?.ZoraCreditsText;
    }

    public override string ToString() => $"{Name}:{World.Id}";
}
