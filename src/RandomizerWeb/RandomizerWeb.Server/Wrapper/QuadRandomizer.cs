using Randomizer.Graph;
using Randomizer.Shared.Contracts;
using System.IO.Hashing;
using System.Text;

namespace RandomizerWeb.Server.Wrapper;

/*
 * This class wraps the ALLTPR Randomizer in a IRandomizer interface used by the SMZ3 randomizer web frontend.
 * This is done so that the SMZ3 randomizer web frontend can be used to generate seeds for the Quad Randomizer based on the ALTTPR code.
 */

public sealed class SingletonRandomizer
{
    private static readonly Lazy<SingletonRandomizer> _lazy =
        new Lazy<SingletonRandomizer>(() => new SingletonRandomizer());

    public static SingletonRandomizer Instance => _lazy.Value;
    public Randomizer.Graph.Randomizer Randomizer { get; init; } = new Randomizer.Graph.Randomizer([new WorldConfig()], 0);
    private SingletonRandomizer() { }
}

public class QuadRandomizer : IRandomizer
{
    public string Id => "quad";

    public string Name => "Quad Randomizer";

    public string Version => Randomizer.Graph.Randomizer.GetVersionString();

    public List<IRandomizerOption> Options => RandomizerOptions.List;

    public ISeedData GenerateSeed(IDictionary<string, string> options, string seed, CancellationToken cancellationToken)
    {
        int randoSeed;
        if (string.IsNullOrEmpty(seed))
        {
            randoSeed = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue);
            seed = randoSeed.ToString();
        }
        else
        {
            randoSeed = int.Parse(seed);
            /* The Random ctor takes the absolute value of a negative seed.
             * This is an non-obvious behavior so we treat a negative value
             * as out of range. */
            if (randoSeed < 0)
                throw new ArgumentOutOfRangeException("Expected the seed option value to be an integer value in the range [0, 2147483647]");
        }

        var randoRnd = new Random(randoSeed);
        var config = RandomizerOptions.Parse(options);

        /* FIXME: Just here to semi-obfuscate race seeds until a better solution is in place */
        if (config.Race)
        {
            randoRnd = new Random(randoRnd.Next());
        }

        var worldConfig = new Randomizer.Graph.WorldConfig
        {
            State = Randomizer.Graph.StateOption.Open,
            Goal = config.Goal switch
            {
                Goal.DefeatAll => Randomizer.Graph.GoalOption.Ganon,
                Goal.TriforceHunt => Randomizer.Graph.GoalOption.TriforceHunt,
                _ => throw new ArgumentOutOfRangeException("Invalid Goal")
            },
            Accessibility = Randomizer.Graph.AccessibilityOption.Items,
            CrystalsTower = config.OpenTower switch
            {
                OpenTower.Random => randoRnd.Next(0, 7),
                _ => (int)config.OpenTower
            },
            CrystalsGanon = config.GanonVulnerable switch
            {
                GanonVulnerable.Random => randoRnd.Next(0, 7),
                _ => (int)config.GanonVulnerable
            },
            SMBosses = config.OpenSMTourian switch
            {
                OpenSMTourian.Random => randoRnd.Next(0, 4),
                _ => (int)config.OpenSMTourian
            },
            Z1Triforces = config.Z1Triforces switch
            {
                Z1Triforces.Random => randoRnd.Next(0, 8),
                _ => (int)config.Z1Triforces
            }
        };

        var randomizer = new Randomizer.Graph.Randomizer([worldConfig], randoSeed);
        randomizer.Randomize();

        var seedData = new SeedData
        {
            Guid = new HexGuid(),
            Seed = seed,
            Game = Name,
            Mode = config.GameMode.ToLowerString(),
            Logic = "Quad",
            Playthrough = [],
            Worlds = new(),
            BasePatchHash = Randomizer.RomModifications.RomWriter.GetBasePatchHash()            
        };

        foreach(var world in randomizer.Worlds)
        {
            var worldData = new WorldData
            {
                Id = world.Id,
                Guid = new HexGuid(),
                Player = "Player " + world.Id,
                Patches = Randomizer.RomModifications.RomWriter.WriteForWorld(world, null, null, null, randomizer.PRNG, null),
                Locations = world.GetLocations().Where(l => l.Item != null && l.Item.Bytes != null && l.Item.Bytes.ContainsKey("z3")).Select(l => new LocationData
                {
                    LocationId = (int)Crc32.HashToUInt32(Encoding.UTF8.GetBytes(l.Name)),
                    ItemId = GetItemId(l.Item!),
                    ItemWorldId = l.World.Id
                }).ToList<ILocationData>(),
                WorldState = null
            };

            seedData.Worlds.Add(worldData);
        }

        return seedData;
    }

    public Dictionary<int, IItemTypeData> GetItems()
    {
        var items = SingletonRandomizer.Instance.Randomizer.Graph.AllItems;
        var itemData = items
            .Where(i => i.Bytes != null && i.Bytes.ContainsKey("z3"))
            .DistinctBy(GetItemId)
            .Select(i => new ItemTypeData
        {
            Id = GetItemId(i),
            Name = i.Name
        }).Cast<IItemTypeData>().ToDictionary(itd => itd.Id);

        itemData.Add(107, new ItemTypeData
        {
            Id = 107,
            Name = "TriforcePiece"
        });

        return itemData;
    }

    public Dictionary<int, ILocationTypeData> GetLocations()
    {
        var locations = SingletonRandomizer.Instance.Randomizer.Graph.GetVertices();
        return locations.DistinctBy(l => l.Name).Select(l => new LocationTypeData
        {
            Id = (int)Crc32.HashToUInt32(Encoding.UTF8.GetBytes(l.Name)),
            Name = l.Name,
            Type = l.Type.ToString(),
            Region = l.Game switch
            {
                Game.SuperMetroid => l.Name.Contains(" - ") ? l.Name.Split(" - ")[1] : "Unknown",
                Game.Zelda => l.Name.Contains(" - ") ?
                    l.Name.Contains(" - Underworld - ") ?
                        l.Name.Split(" - ")[2] :
                        l.Name.Split(" - ")[1] : "Unknown",
                Game.Metroid => l.Name.Contains(" - ") ? l.Name.Split(" - ")[1] : "Unknown",
                _ => l.Name.Contains(" - ") ? l.Name.Split(" - ")[0] : "Overworld"

            },
            Area = l.Game switch
            {
                Game.Alttp => "A Link to the Past",
                Game.SuperMetroid => "Super Metroid",
                Game.Zelda => "Zelda 1",
                Game.Metroid => "Metroid",
                _ => "A Link to the Past"
            }
        }).Cast<ILocationTypeData>().ToDictionary(ltd => ltd.Id);
    }

    private int GetItemId(Item item)
    {
        if(item == null || item.Bytes == null || !item.Bytes.ContainsKey("z3"))
        {
            return -1;
        }

        var itemBytes = item.Bytes["z3"]!;

        return itemBytes.Length switch
        {
            1 => itemBytes[0],
            4 => itemBytes[2] + 0x100,
            6 => itemBytes[5] switch
            {
                0x20 => itemBytes[0] + 0x200,
                _ => itemBytes[5] + 0x300
            },
            _ => throw new ArgumentOutOfRangeException("Invalid item byte length")
        };
    }
}

public class RandomizerOption : IRandomizerOption
{
    public string Key { get; set; }
    public string Description { get; set; }
    public RandomizerOptionType Type { get; set; }
    public Dictionary<string, string> Values { get; set; }
    public string Default { get; set; }
}

public class SeedData : ISeedData
{
    public string Guid { get; set; }
    public string Seed { get; set; }
    public string Game { get; set; }
    public string Logic { get; set; }
    public string Mode { get; set; }
    public string BasePatchHash { get; set; }
    public List<IWorldData> Worlds { get; set; }
    public List<Dictionary<string, string>> Playthrough { get; set; }
}

public class WorldData : IWorldData
{
    public int Id { get; set; }
    public string Guid { get; set; }
    public string Player { get; set; }
    public Dictionary<int, byte[]> Patches { get; set; }
    public List<ILocationData> Locations { get; set; }
    public object WorldState { get; set; }
}

public class LocationData : ILocationData
{
    public int LocationId { get; set; }
    public int ItemId { get; set; }
    public int ItemWorldId { get; set; }
}

public class ItemTypeData : IItemTypeData
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class LocationTypeData : ILocationTypeData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Region { get; set; }
    public string Area { get; set; }
}

public class HexGuid
{
    public Guid Guid { get; } = Guid.NewGuid();
    public override string ToString() => Guid.ToString();
    public static implicit operator string(HexGuid hexGuid) => hexGuid.ToString();
}
