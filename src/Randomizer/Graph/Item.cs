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
    public Dictionary<string, byte[]?> Bytes { get; }
    public Game Game { get; }

    /// <summary>
    /// Create a new Item.
    /// </summary>
    ///
    /// <param name="name">Unique name of item</param>
    /// <param name="world">World this item is in</param>
    public Item(string name, World world, Game game = Game.Alttp)
    {
        Name = name;
        World = world;
        Game = game;

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
