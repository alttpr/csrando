namespace Randomizer.Games.SuperMetroid.Model;

public record BossScenarioCollection(BossScenario[] Scenarios);

public record BossScenario
(
    int Id,
    string Name,
    string Boss,
    string[]? ExplicitWeapons,
    string[]? ExcludedWeapons,
    Requirement Requires,
    int? BossDodgeRate,
    int? AttackOpportunityDuration,
    DamageWindow[]? DamageWindows,
    IncomingDamage[]? IncomingDamage,
    int? ParticleFrequencyFrames,
    Note? Note,
    Note? DevNote
);

public record DamageWindow
(
    string Name,
    decimal WindowPercent,
    Requirement Requires,
    Note? Note,
    Note? DevNote
);

public record IncomingDamage
(
    string Name,
    string Attack,
    int FrequencyFrames,
    Requirement AvoidingRequires,
    Note? Note,
    Note? DevNote
);
