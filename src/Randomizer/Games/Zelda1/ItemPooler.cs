using Randomizer.Graph;

namespace Randomizer.Games.Zelda1;

/// <summary>Get the sets of items to place.</summary>
/// <param name="worlds">worlds to get Item pools for</param>
internal sealed class ItemPooler : IItemPooler
{

    private readonly PRNG _prng;

    public ItemPooler(IWorld[] worlds, PRNG prng)
    {
        _prng = prng;
        Pool = [.. worlds.OfType<World>().SelectMany(GetPoolForWorld)];
        SetLocations = BuildLocations(worlds);
    }

    private SetLocations BuildLocations(IWorld[] worlds)
    {
        var setLocations = new SetLocations();
        foreach (var vertex in worlds.SelectMany(world => world.GetLocations()).OfType<Vertex>())
        {
            if (vertex.Type == VertexType.Item)
            {
                setLocations.Add(vertex, [ItemSetName.DefaultSet, .. vertex.ItemSet]);
            }
        }
        return setLocations;
    }

    /// <summary>Get a list possible locations, keyed by item set.</summary>
    public SetLocations SetLocations { get; }
    /// <summary>Get list of all items in their weighted sets.</summary>
    public PooledItem[] Pool { get; }

    /// <summary>Get list of all items for <paramref name="world"/> in their weighted sets.</summary>
    private List<PooledItem> GetPoolForWorld(World world)
    {
        List<PooledItem> worldSet =
        [
            new PooledItem(new ItemSetName("z1d1", world), 1, world.GetItem("Map")),
            new PooledItem(new ItemSetName("z1d1", world), 1, world.GetItem("Compass")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d1", world), 1, world.GetItem("Key")), 4),

            new PooledItem(new ItemSetName("z1d2", world), 1, world.GetItem("Map")),
            new PooledItem(new ItemSetName("z1d2", world), 1, world.GetItem("Compass")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d2", world), 1, world.GetItem("Key")), 3),

            new PooledItem(new ItemSetName("z1d3", world), 1, world.GetItem("Map")),
            new PooledItem(new ItemSetName("z1d3", world), 1, world.GetItem("Compass")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d3", world), 1, world.GetItem("Key")), 4),

            new PooledItem(new ItemSetName("z1d4", world), 1, world.GetItem("Map")),
            new PooledItem(new ItemSetName("z1d4", world), 1, world.GetItem("Compass")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d4", world), 1, world.GetItem("Key")), 3),

            new PooledItem(new ItemSetName("z1d5", world), 1, world.GetItem("Map")),
            new PooledItem(new ItemSetName("z1d5", world), 1, world.GetItem("Compass")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d5", world), 1, world.GetItem("Key")), 5),

            new PooledItem(new ItemSetName("z1d6", world), 1, world.GetItem("Map")),
            new PooledItem(new ItemSetName("z1d6", world), 1, world.GetItem("Compass")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d6", world), 1, world.GetItem("Key")), 4),

            new PooledItem(new ItemSetName("z1d7", world), 1, world.GetItem("Map")),
            new PooledItem(new ItemSetName("z1d7", world), 1, world.GetItem("Compass")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d7", world), 1, world.GetItem("Key")), 3),

            new PooledItem(new ItemSetName("z1d8", world), 1, world.GetItem("Map")),
            new PooledItem(new ItemSetName("z1d8", world), 1, world.GetItem("Compass")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d8", world), 1, world.GetItem("Key")), 4),

            new PooledItem(new ItemSetName("z1d9", world), 1, world.GetItem("Map")),
            new PooledItem(new ItemSetName("z1d9", world), 1, world.GetItem("Compass")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d9", world), 1, world.GetItem("Key")), 2),

            new PooledItem(ItemSetName.DefaultSet, 4, world.GetItem("SwordL1")),

            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Bombs")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("StepLadder")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Raft")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Recorder")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SwordL2")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SwordL3")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("BlueCandle")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("RedCandle")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SilverArrows")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Bow")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Arrows")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("MagicalKey")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Rod")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Book")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("BlueRing")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("RedRing")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("PowerBracelet")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Letter")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("MagicShield")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Boomerang")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("MagicBoomerang")),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("HeartContainer")), 9),

            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("HeartContainer")), 4),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Bombs")), 20),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Key")), 8),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Rupee")), 5),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Rupee5")), 11),
        ];

        return worldSet;
    }
}
