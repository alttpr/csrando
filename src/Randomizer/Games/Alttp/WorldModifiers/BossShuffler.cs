using Microsoft.Extensions.Logging;
using Randomizer.Graph;
using BaseVertex = Randomizer.Graph.Vertex;

namespace Randomizer.Games.Alttp.WorldModifiers;

/// <summary>Modify the edges of the graph to place bosses.</summary>
internal sealed class BossShuffler : IAlttpWorldModifier
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private const string BOSS_SHUFFLER_ITEM_CONDITION = "bossKillShufflerOverwrite";
    private static readonly Dictionary<string, string> VANILLA_BOSSES = new()
    {
        { "Eastern Palace - Boss Room", "DefeatArmosKnight" },
        { "Desert Palace - Boss Room", "DefeatLanmolas" },
        { "Tower Of Hera - Boss Room", "DefeatMoldorm" },
        { "Palace of Darkness - Boss Room", "DefeatHelmasaur" },
        { "Swamp Palace - Boss Room", "DefeatArrghus" },
        { "Skull Woods - Boss Room", "DefeatMothula" },
        // TODO: this one deviates because of bringing the maiden to the boss room.
        { "Thieves' Town - Boss Room - Blind Active", "DefeatBlind" },
        //{ "Thieves' Town - Boss Room", "DefeatBlind" },
        { "Ice Palace - Boss Room", "DefeatKholdstare" },
        { "Misery Mire - Boss Room", "DefeatVitreous" },
        { "Turtle Rock - Boss Room", "DefeatTrinexx" },
        { "Ganon's Tower - Ice Room", "DefeatArmosKnight" },
        { "Ganon's Tower - Lanmolas", "DefeatLanmolas" },
        { "Ganon's Tower - Moldorm - Kill Zone", "DefeatMoldorm" },
    };

    /// <summary>Swap Entrances based on world settings.</summary>
    public void AdjustEdges(World world, PRNG prng)
    {
        var bossRooms = world.GetLocations()
            .Where(v => v.World == world
                     && v.Edges.Any(e => e.Condition.Item.Name == BOSS_SHUFFLER_ITEM_CONDITION))
            .ToList();

        // force Kholdstare for swordless to be in Ice Palace
        // we'd like to have him elsewhere, but that requires us to make Bombos work in the room and put a tile down first.
        if (world.Config.Weapon == WeaponOption.Swordless)
        {
            foreach (var bossRoom in bossRooms)
            {
                if (VANILLA_BOSSES[bossRoom.Name] == "DefeatKholdstare")
                    continue;

                bossRoom.Edges.RemoveAll(e => e.Condition.Item.Name == "DefeatKholdstare");
            }
        }

        // most restrictive first
        // TODO: if we make bosses more explicit, this (plus the uses of Allow that follow)
        //       needs to be changed as well (see DataLoader where Region.Bosses is used)
        var bossLocations = bossRooms.OfType<Vertex>().OrderBy(v => v.Allow!.Length);

        List<string> placeBosses;
        switch (world.Config.BossShuffle)
        {
            case BossShuffleOption.Random:
                placeBosses =
                [
                    "DefeatArmosKnight",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                    "DefeatHelmasaur",
                    "DefeatArrghus",
                    "DefeatMothula",
                    "DefeatBlind",
                    "DefeatKholdstare",
                    "DefeatVitreous",
                    "DefeatTrinexx",
                ];
                foreach (var location in bossLocations)
                {
                    IEnumerable<string> bosses = placeBosses;
                    if (location is Vertex { Allow: string[] acceptableBosses })
                        bosses = bosses.Intersect(acceptableBosses);
                    string boss = prng.Shuffle(bosses).First();
                    PlaceBossItemInLocation(boss, location, world);
                }
                break;
            case BossShuffleOption.Full: // 1 copy of each, +3 other copies
                placeBosses =
                [
                    "DefeatArmosKnight",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                    "DefeatHelmasaur",
                    "DefeatArrghus",
                    "DefeatMothula",
                    "DefeatBlind",
                    "DefeatKholdstare",
                    "DefeatVitreous",
                    "DefeatTrinexx",
                ];
                placeBosses.AddRange(prng.Shuffle(placeBosses).Take(3));

                foreach (var location in bossLocations)
                {
                    IEnumerable<string> bosses = placeBosses;
                    if (location is Vertex { Allow: string[] acceptableBosses })
                        bosses = bosses.Intersect(acceptableBosses);
                    string boss = prng.Shuffle(bosses).First();
                    placeBosses.Remove(boss);
                    PlaceBossItemInLocation(boss, location, world);
                }
                break;
            case BossShuffleOption.Simple: // 1:1
                placeBosses =
                [
                    "DefeatArmosKnight",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                    "DefeatHelmasaur",
                    "DefeatArrghus",
                    "DefeatMothula",
                    "DefeatBlind",
                    "DefeatKholdstare",
                    "DefeatVitreous",
                    "DefeatTrinexx",
                    "DefeatArmosKnight",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                ];

                foreach (var location in bossLocations)
                {
                    IEnumerable<string> bosses = placeBosses;
                    if (location is Vertex { Allow: string[] acceptableBosses })
                        bosses = bosses.Intersect(acceptableBosses);
                    string boss = prng.Shuffle(bosses).First();
                    placeBosses.Remove(boss);
                    PlaceBossItemInLocation(boss, location, world);
                }
                break;
            case BossShuffleOption.None:
            default:
                foreach (var location in bossLocations)
                {
                    string boss = VANILLA_BOSSES[location.Name];
                    PlaceBossItemInLocation(boss, location, world);
                }
                break;
        }
    }

    /// <summary>Place Boss item in location.</summary>
    /// <param name="bossItem">Boss item name</param>
    /// <param name="location">Location name</param>
    /// <param name="world">World</param>
    /// <exception cref="Exception">If can't place boss in location</exception>
    private static void PlaceBossItemInLocation(string bossItem, BaseVertex from, World world)
    {
        if (from is null || from is not Vertex alttpVertex)
            throw new Exception("Can't place boss.");

        var worldBossItem = world.GetItem(bossItem);
        var bossVertex = (Vertex)world.Graph.AddVertex(new Vertex
        {
            Name = $"{from.Name} - Boss",
            Type = VertexType.Boss,
            World = world,
            RoomOffset = alttpVertex.RoomOffset,
            RoomId = alttpVertex.RoomId,
            RoomOAM = alttpVertex.RoomOAM,
            // TODO: do we need more in here?
        });
        world.Graph.AddDirected(from, bossVertex, worldBossItem);

        var bossEdge = from.Edges.Find(e => e.Condition.Item.Name == BOSS_SHUFFLER_ITEM_CONDITION);
        if (bossEdge is null)
            throw new Exception($"Can't place boss in {from.Name}, missing the boss connection with condition {BOSS_SHUFFLER_ITEM_CONDITION}.");

        // attach Mob-type vertices to the boss, so the rom writer can place them.
        var bossSprites = YamlReader.LoadBossSprites();
        var fixedCondition = new ItemCondition(world.GetItem("fixed"), 1);
        foreach (var bossSprite in bossSprites[bossItem])
        {
            var spriteVertex = world.Graph.AddVertex(new Vertex
            {
                Name = $"{bossVertex.Name} - {bossSprite.Name}",
                Type = VertexType.Mob,
                World = world,
                // TODO: which of those do we actually need? and does this break things if we have multiple ones with the same values?
                Position = bossVertex.RoomOffset + bossSprite.Position,
                Addresses = bossVertex.Addresses,
                Group = bossVertex.Group,
                Item = bossVertex.Item,
                ItemSet = bossVertex.ItemSet,
                Offset = bossVertex.Offset,
                RoomId = bossVertex.RoomId,
                RoomOAM = bossVertex.RoomOAM,
                Sheets = bossVertex.Sheets,
                Sprite = Sprite.Get(bossSprite.Sprite),
            });
            world.Graph.AddDirected(bossVertex, spriteVertex, fixedCondition);
        }

        bossEdge.Condition = new ItemCondition(worldBossItem, 1);
        _logger.LogInformation("[BS] Placing {Boss} in '{Location}'", bossItem.Replace("Defeat", ""), from.Name);
    }
}
