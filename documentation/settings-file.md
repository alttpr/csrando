The command line option `settings` accepts a serialized version of [`WorldConfig`](/src/Randomizer/Graph/WorldConfig.cs)
(either as array, one per world; or as object, which is duplicated for every world when `multiworld` is set to a number other than 1.)

Other parameters affecting world configuration are **ignored** when a settings file is used.

## JSON
JSON is a supported format for the settings file, with the following traits:
- Enumerations are serialized as string, rather than numeric value.
- Numbers are allowed to be quoted (as JSON string.)
- Individual property names are case-insensitive (but they may not use alternate styles such as kebab-case or snake_case.)
- Optional properties may be omitted and will use the default value.

### 2-World multiworld sample
The following example passes two worlds with individual configuration:
```json
[
    {
        "Language": "en",
        "Alttp": {
            "RomHardMode": 0,
            "CrystalsGanonChoices": ["0", "7"],
            "CrystalsTowerChoices": ["3"],
            "Goal": "Ganon",
            "Accessibility": "Items",
            "State": "Open",
            "Weapon": "Randomized",
            "RegionShopSupply": "Normal",
            "RegionWildKeys": false,
            "RegionWildBigKeys": false,
            "RegionWildMaps": false,
            "RegionWildCompasses": false,
            "RomRupeeBow": false,
            "MapOnPickup": false,
            "EscapeAssist": false,
            "PseudoBoots": false,
            "FastRom": true,
            "QuickSwap": false,
            "HudItemCounter": true,
            "HeartColor": "Red",
            "HeartBeepSpeed": "Half",
            "MenuSpeed": "Normal",
            "GanonAgahnimRNG": "Table",
            "SilversAutoEquip": "Collection",
            "CompassCounter": "Off"
        }
    },
    {
        "Language": "de",
        "Alttp": {
            "RomHardMode": 1,
            "CrystalsGanon": "3",
            "CrystalsTower": "3",
            "Goal": "Ganon",
            "Accessibility": "Items",
            "State": "Open",
            "Glitches": "None",
            "Techs": [],
            "Weapon": "Vanilla",
            "EntranceShuffle": "None",
            "EnemyShuffle": "None",
            "BossShuffle": "None",
            "RegionShopSupply": "Normal",
            "RegionWildKeys": true,
            "RegionWildBigKeys": true,
            "RegionWildMaps": false,
            "RegionWildCompasses": false,
            "RomRupeeBow": true,
            "MapOnPickup": false,
            "EscapeAssist": false,
            "PseudoBoots": false,
            "FastRom": true,
            "QuickSwap": true,
            "GoalIcon": "Triforce",
            "GenericKeys": false,
            "HudItemCounter": true,
            "HeartColor": "Random",
            "HeartBeepSpeed": "Half",
            "MenuSpeed": "Instant",
            "GanonAgahnimRNG": "Table",
            "SilversAutoEquip": "Collection",
            "CompassCounter": "On"
        }
    }
]
```
