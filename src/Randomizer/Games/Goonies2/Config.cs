namespace Randomizer.Games.Goonies2;

using System.Collections.Generic;

public class Config
{
    public EntranceShuffleOption EntranceShuffle { get; init; } = EntranceShuffleOption.None;

    public EnemyShuffleOption EnemyShuffle { get; init; } = EnemyShuffleOption.None;

    public EnemyDamageOption EnemyDamage { get; init; } = EnemyDamageOption.Default;

    public EnemyHealthOption EnemyHealth { get; init; } = EnemyHealthOption.Default;

    public List<string> StartingEquipment { get; init; } = new();

    public bool AnnieShuffle { get; init; } = false;
    public bool GoonieShuffle { get; init; } = false;
    public bool ItemShuffle { get; init; } = false;
}

public enum EntranceShuffleOption { None, Simple, Restricted, Full, Crossed, Insanity }
public enum EnemyShuffleOption { None, Shuffled, Random }
public enum EnemyDamageOption { Default, Shuffled, Random }
public enum EnemyHealthOption { Default, Easy, Medium, Hard, Expert }
