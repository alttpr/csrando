namespace Randomizer.Games.SuperMetroid.Model;
public record WeaponCollection(
    Weapon[] Weapons
);

public record Weapon(
    int Id,
    string Name,
    int Damage,
    int CooldownFrames,
    Requirement UseRequires,
    Requirement ShotRequires,
    bool Situational,
    bool HitsGroup,
    string[] Categories,
    Note? Note,
    Note? DevNote
);
