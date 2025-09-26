namespace Randomizer.Games.Combo;

using Randomizer.Graph;

public enum ItemType
{
    Meta,
}

public sealed class Item : Randomizer.Graph.Item
{
    public ItemType Type { get; }

    /// <summary>
    /// Create a new Item.
    /// </summary>
    ///
    /// <param name="name">Unique name of item</param>
    /// <param name="world">World this item is in</param>
    public Item(string name, IWorld world)
        : base(name, world)
    {
        Type = ItemType.Meta;
        Bytes = null;
    }
}
