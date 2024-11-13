namespace Randomizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

using System.Text.Json.Serialization;
using Randomizer.Graph;
using Randomizer.Games.Alttp;


[YamlStaticContext]
[YamlSerializable(typeof(Games.Alttp.YamlItem))]
[YamlSerializable(typeof(Games.Alttp.Vertices))]
[YamlSerializable(typeof(Games.Alttp.Map))]
[YamlSerializable(typeof(Games.Alttp.Room))]
[YamlSerializable(typeof(Games.Alttp.MapNodes))]
[YamlSerializable(typeof(Games.Alttp.Region))]
[YamlSerializable(typeof(Games.Alttp.Entrance))]
[YamlSerializable(typeof(Games.Alttp.Entity))]
[YamlSerializable(typeof(Games.Alttp.Position))]
[YamlSerializable(typeof(Games.Alttp.Hole))]
[YamlSerializable(typeof(Games.Alttp.ItemEntry))]
[YamlSerializable(typeof(Games.Alttp.Warp))]
[YamlSerializable(typeof(Games.Alttp.MetaEntry))]
[YamlSerializable(typeof(Games.Alttp.Prizepack))]
[YamlSerializable(typeof(Games.Alttp.RoomNodes))]
[YamlSerializable(typeof(Games.Alttp.InventoryEntry))]
[YamlSerializable(typeof(Games.Alttp.YamlSprite))]
[YamlSerializable(typeof(Games.Alttp.DirectedUndirectedPair))]
[YamlSerializable(typeof(Games.Alttp.Entrances))]
public partial class RandomizerStaticContext : YamlDotNet.Serialization.StaticContext 
{

}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    AllowTrailingCommas = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(WorldConfig))]
[JsonSerializable(typeof(WorldConfig[]))]
[JsonSerializable(typeof(Games.Alttp.Config), TypeInfoPropertyName = "AlttpConfig")]
[JsonSerializable(typeof(Games.Goonies2.Config), TypeInfoPropertyName = "Goonies2Config")]
[JsonSerializable(typeof(Games.Zelda1.Config), TypeInfoPropertyName = "Zelda1Config")]
[JsonSerializable(typeof(Games.Alttp.EntranceShuffleOption), TypeInfoPropertyName = "AlttpEntranceShuffleOption")]
[JsonSerializable(typeof(Games.Alttp.EnemyShuffleOption), TypeInfoPropertyName = "AlttpEnemyShuffleOption")]
[JsonSerializable(typeof(Games.Alttp.EnemyDamageOption), TypeInfoPropertyName = "AlttpEnemyDamageOption")]
[JsonSerializable(typeof(Games.Alttp.EnemyHealthOption), TypeInfoPropertyName = "AlttpEnemyHealthOption")]
[JsonSerializable(typeof(Games.Goonies2.EntranceShuffleOption), TypeInfoPropertyName = "Goonies2EntranceShuffleOption")]
[JsonSerializable(typeof(Games.Goonies2.EnemyShuffleOption), TypeInfoPropertyName = "Goonies2EnemyShuffleOption")]
[JsonSerializable(typeof(Games.Goonies2.EnemyDamageOption), TypeInfoPropertyName = "Goonies2EnemyDamageOption")]
[JsonSerializable(typeof(Games.Goonies2.EnemyHealthOption), TypeInfoPropertyName = "Goonies2EnemyHealthOption")]
[JsonSerializable(typeof(Games.Zelda1.EntranceShuffleOption), TypeInfoPropertyName = "Zelda1EntranceShuffleOption")]
public partial class RandomizerJsonStaticContext : JsonSerializerContext
{

}

