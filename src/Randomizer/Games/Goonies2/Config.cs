namespace Randomizer.Games.Goonies2;

using Randomizer.Games.Metadata;

[TargetGame(Game.Goonies2)]
public class Config
{
    [Ignore("Not implemented")]
    public EntranceShuffleOption EntranceShuffle { get; init; } = EntranceShuffleOption.None;

    public EnemyShuffleOption EnemyShuffle { get; init; } = EnemyShuffleOption.None;

    [Ignore("Not implemented")]
    public EnemyDamageOption EnemyDamage { get; init; } = EnemyDamageOption.Default;

    [Ignore("Not implemented")]
    public EnemyHealthOption EnemyHealth { get; init; } = EnemyHealthOption.Default;

    [Ignore("Starting inventory is too advenced to be represented with simple attributes")]
    public List<string> StartingEquipment { get; init; } = [];

    [Ignore("Not implemented")]
    public bool AnnieShuffle { get; init; } = false;
    [Ignore("Not implemented")]
    public bool GoonieShuffle { get; init; } = false;
    [Ignore("Not implemented")]
    public bool ItemShuffle { get; init; } = false;
}

public enum EntranceShuffleOption { None, Simple, Restricted, Full, Crossed, Insanity }
public enum EnemyShuffleOption { None, Shuffled, Random }
public enum EnemyDamageOption { Default, Shuffled, Random }
public enum EnemyHealthOption { Default, Easy, Medium, Hard, Expert }
