namespace Randomizer.Games.SuperMetroid.Model;

public record EnemyCollection(Enemy[] Enemies);

public record Enemy
(
    int Id,
    string Name,
    Attack[] Attacks,
    int Hp,
    int AmountOfDrops,
    Note? Note,
    Note? DevNote,
    EnemyDrops Drops,
    EnemyDrops? FarmableDrops,
    Dimension Dims,
    bool Freezable,
    bool Grapplable,
    string[] Invul,
    DamageMultiplier[] DamageMultipliers,
    string[]? Areas
);

public record DamageMultiplier
(
    string Weapon,
    decimal Value
);

public record Dimension
(
    int W,
    int H,
    Note? Note,
    Note? DevNote
);

public record Attack
(
    string Name,
    int BaseDamage,
    bool? AffectedByVaria,
    bool? AffectedByGravity
);

public record EnemyDrops
(
    decimal NoDrop,
    decimal SmallEnergy,
    decimal BigEnergy,
    decimal Missile,
    decimal Super,
    decimal PowerBomb
);
