namespace Randomizer.Games.SuperMetroid;

using Randomizer.Graph;

public enum ItemType
{
    Meta,
}

public sealed class Item : Randomizer.Graph.Item
{
    public ItemType Type { get; }

    // items.json is an upstream sm-json-data snapshot, so tier declarations live in
    // a sidecar file instead. Unlisted names (upgrades, keycards, tokens, flags)
    // keep the Major default from the base class.
    private static readonly Lazy<Dictionary<string, string>> tierData = new(() =>
        System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
            File.ReadAllText(Path.Combine(Model.JsonReader.DataRoot, "item_tiers.json")))
            ?? []);

    /// <summary>
    /// Create a new Item.
    /// </summary>
    ///
    /// <param name="name">Unique name of item</param>
    /// <param name="world">World this item is in</param>
    public Item(string name, IWorld world)
        : base(name, world)
    {
        Tier = ItemTiers.ParseDeclaration(tierData.Value.GetValueOrDefault(name));
    }
}
