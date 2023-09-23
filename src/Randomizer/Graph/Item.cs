namespace AlttpRandomizer.Graph;

/**
 * An Item is any collectable thing in game.
 */
public sealed class Item
{
    public string Name { get; }
    public string NiceName { get; }
    public string I18NName { get; }
    public World World { get; }
    public bool Meta { get; private set; }

    /**
     * Create a new Item.
     *
     * @param string name Unique name of item
     * @param int[]|null[] bytes data to write to Location addresses
     * @param int world_id world for which the item belongs
     *
     * @return void
     */
    public Item(string name, World world)
    {
        Name = name;
        I18NName = "item." + name;
        string? formatted = __(I18NName);
        NiceName = formatted ?? "";
        World = world;
    }
    private static string? __(string name)
    {
        return null; // TODO: this should call localization and return the translated name
    }

    /**
     * serialized version of Item.
     *
     * @return string
     */
    public override string ToString()
    {
        return $"{Name}:{World.Id}";
    }
}
