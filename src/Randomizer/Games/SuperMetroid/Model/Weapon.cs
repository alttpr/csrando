namespace Randomizer.Games.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
