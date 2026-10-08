namespace Randomizer.Games.Alttp;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Randomizer.Games.Alttp.WorldModifiers;
using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseVertex = Randomizer.Graph.Vertex;

public delegate string? GetItemTextOverride(IItem? item, WorldConfig worldConfig);
public static class RomWriter
{
    private static readonly HeartColorOption[] _heartColorOptions = [HeartColorOption.Blue, HeartColorOption.Green, HeartColorOption.Yellow, HeartColorOption.Red];
    public static void Write(IRom baseRom, World world, PRNG prng, int offset = 0, GetItemTextOverride? getText = null)
    {
        var rom = new Rom(baseRom, world.WorldConfig.Language, offset);

        var config = world.Config;
        var heartColor = config.HeartColor;
        if (heartColor == HeartColorOption.Random)
            heartColor = prng.GetRandomElement(_heartColorOptions);
        rom.SetHeartColors(heartColor);
        rom.SetHeartBeepSpeed(config.HeartBeepSpeed);

        rom.SetQuickSwap(config.QuickSwap);

        WriteWorld(world, rom, prng, getText);

        rom.MuteMusic(config.NoMusic);
        rom.SetMenuSpeed(config.MenuSpeed);

        // TODO: patch in the sprite
        // TODO: tournament mode
    }

    private static void WriteWorld(World world, Rom rom, PRNG prng, GetItemTextOverride? getText)
    {
        var config = world.Config;

        if (!false) //config("multiworld", false))
        {
            var itemLocations = world.GetLocationsOfType(VertexType.Item).ToArray();
            // replace one of the bows with the alternate, so we can give a silvers hint at the end.
            var alternateBowLocation = itemLocations
                .Where(v => v.Item?.Name == "ProgressiveBow")
                .Skip(1)
                .FirstOrDefault();
            if (alternateBowLocation != null)
                alternateBowLocation.Item = world.GetItem("ProgressiveBowAlternate");

            var nothing = world.GetItem("Nothing");
            foreach (var location in itemLocations.OfType<Vertex>())
            {
                var itemToWrite = location.Item as Item ?? nothing;

                rom.WriteItem(location, itemToWrite);
                SetCreditsText(rom, world.WorldConfig, location, location.Item, getText);
                rom.WriteDungeonMusic(location, itemToWrite, prng);
                SetHintText(rom, world.WorldConfig, location, location.Item, getText);
                rom.WriteLocationSpecificData(world, location, itemToWrite);
            }
        }

        if (config.State == StateOption.Standard)
            SetEscapeFills(world, rom);

        rom.SetGoalRequiredCount((config.Goal == GoalOption.TriforceHunt || config.Goal == GoalOption.Trifecta) ? config.GoalRequiredCount : (ushort)0);
        rom.SetGoalIcon(config.GoalIcon);

        // Set item functionality settings
        rom.SetCaneOfByrnaSpikeCaveUsage();
        rom.SetCapeSpikeCaveUsage();
        rom.SetByrnaCaveSpikeDamage(0x08);
        rom.SetCaneOfByrnaMagicPerCycle();

        rom.SetCapeRegularMagicUsage(
            config.CapeMagicUsageNormal,
            config.CapeMagicUsageHalf,
            config.CapeMagicUsageQuarter
        );
        rom.SetCaneOfByrnaInvulnerability(config.CaneOfByrnaInvulnerability);
        rom.SetPowderedSpriteFairyPrize(config.PowderedSpriteFairyPrize);
        rom.SetBottleFills(
            config.BottleFillHealth,
            config.BottleFillMagic
        );
        rom.SetCatchableFairies(config.CatchableFairies);
        rom.SetCatchableBees(config.CatchableBees);
        rom.SetStunItems(config.StunItemsHookshot, config.StunItemsBoomerang);
        rom.SetSilversOnlyAtGanon(config.SilversOnlyAtGanon);

        rom.SetRupoorValue(0); //item.value.Rupoor

        rom.SetGanonAgahnimRng(config.GanonAgahnimRNG);

        rom.SetTowerCrystalRequirement(int.Parse(config.CrystalsTower));
        rom.SetGanonCrystalRequirement(int.Parse(config.CrystalsGanon));
        rom.SetMoldormEyeCount(config.MoldormEyeCount, config.Molderp);

        // testing features
        rom.SetGenericKeys(config.GenericKeys);
        //rom.SetupCustomShops(getShops());
        rom.SetRupeeArrow(config.RomRupeeBow);
        rom.SetWishingWellChests(true);
        rom.SetWishingWellUpgrade(false);
        rom.SetHyliaFairyShop(true);
        rom.SetRestrictFairyPonds(true);
        rom.SetLimitProgressiveSword(
            4, //item.overflow.count.Sword
            0x47 //item.overflow.replacement.Sword; TwentyRupees2
        );
        rom.SetLimitProgressiveShield(
            3, //item.overflow.count.Shield
            0x47 //item.overflow.replacement.Shield; TwentyRupees2
        );
        rom.SetLimitProgressiveArmor(
            2, //item.overflow.count.Armor
            0x47 //item.overflow.replacement.Armor; TwentyRupees2
        );
        rom.SetLimitBottle(
            4, //item.overflow.count.Bottle
            0x47 //item.overflow.replacement.Bottle; TwentyRupees2
        );
        rom.SetLimitProgressiveBow(
            2, //item.overflow.count.Bow
            0x47 //item.overflow.replacement.Bow; TwentyRupees2
        );

        rom.SetSilversEquip(config.SilversAutoEquip);
        rom.SetSubstitutions([
            // lamp -> 5 rupees
            0x12,
            0x01,
            0x35,
            0xFF,
            // 6 +5 bomb upgrades -> +10 bomb upgrade
            0x51,
            0x06,
            0x52,
            0xFF,
            // 6 +5 arrow upgrades -> +10 arrow upgrade
            0x53,
            0x06,
            0x54,
            0xFF,
            // silver arrows -> 1 arrow
            0x58,
            0x01,
            (byte)(config.RomRupeeBow ? 0x36 : 0x43),
            0xFF,
            // boss heart -> 20 rupees
            0x3E,
            10, //item.overflow.count.BossHeartContainer
            0x47, //item.overflow.replacement.BossHeartContainer; TwentyRupees2
            0xFF,
            // piece of heart -> 20 rupees
            0x17,
            24, //item.overflow.count.PieceOfHeart
            0x47, //item.overflow.replacement.PieceOfHeart; TwentyRupees2
            0xFF,
        ]);

        switch (config.Goal)
        {
            case GoalOption.TriforceHunt:
                rom.EnableTriforceTurnIn(true);
                // intentional fall-thru
                goto case GoalOption.Pedestal;
            case GoalOption.Pedestal:
                rom.SetGanonInvincible("yes");
                break;
            case GoalOption.Dungeons:
                rom.SetGanonInvincible("dungeons");
                break;
            //case GoalOption.GanonHunt:
            //    rom.InitialSram.PreOpenPyramid();
            //    rom.SetGanonInvincible("triforce_pieces");
            //    break;
            case GoalOption.FastGanon:
                rom.InitialSram.PreOpenPyramid();
                rom.SetGanonInvincible("crystals_only");
                break;
            //case GoalOption.Completionist:
            //    rom.SetGanonInvincible("completionist");
            //    break;
            case GoalOption.Trifecta:
                rom.EnableTriforceTurnIn(true);
                rom.SetGanonInvincible("crystals_only");
                break;

            default:
                rom.SetGanonInvincible("crystals_only");
                break;
        }

        SetProgressionText(world, rom, prng);
        SetHintText(world, rom, prng, getText);
        SetCreditsText(world, rom, prng);

        rom.SetMapMode(config.MapOnPickup);
        rom.SetCompassMode(config.CompassCounter);
        rom.SetCompassCountTotals();
        rom.SetFreeItemTextMode(freeCrystals: false, config.RegionWildKeys, config.RegionWildBigKeys, config.RegionWildMaps, config.RegionWildCompasses, config.RegionWildKeys);
        rom.SetFreeItemMenu(config.RegionWildKeys, config.RegionWildBigKeys, config.RegionWildMaps, config.RegionWildCompasses);
        rom.SetDiggingGameRng((byte)prng.GetRandomInt(1..30));

        rom.WriteRNGBlock(() => (byte)prng.GetRandomInt(0, 0x100));

        WritePrizePacksToRom(world, rom);
        WriteEntrancesToRom(world, rom);

        WriteEnemyDamageToRom(world, rom, prng);
        WriteEnemyHealthToRom(world, rom, prng);
        WriteEnemiesToRom(world, rom);

        rom.SetPersistentFloodGate(config.EntranceShuffle != EntranceShuffleOption.None);
        rom.SetSmithyFreeTravel(config.EntranceShuffle != EntranceShuffleOption.None);
        if (config.EntranceShuffle != EntranceShuffleOption.None)
            rom.PreOpenBombableWalls();

        rom.SetPyramidFairyChests(true); //region.swordsInPool
        rom.SetSmithyQuickItemGive(true); //region.swordsInPool

        rom.SetGameState(config.State);
        rom.SetSwordlessMode(config.Weapon == WeaponOption.Swordless);
        if (config.State != StateOption.Inverted)
        {
            switch (config.Glitches) //rom.logicMode
            {
                case GlitchesOption.Major:
                //case GlitchesOption.HybridMajor:
                case GlitchesOption.NoLogic:
                case GlitchesOption.Overworld:
                    rom.SetLockAgahnimDoorInEscape(false);
                    break;
                case GlitchesOption.None:
                default:
                    rom.SetLockAgahnimDoorInEscape(true);
                    break;
            }
        }

        var linksUncleItem = world.GetLocation("Link's Uncle")?.Item;
        if (linksUncleItem != null)
        {
            if (!linksUncleItem.Name.Contains("Sword"))
                rom.RemoveUnclesSword();
            if (!linksUncleItem.Name.Contains("Shield"))
                rom.RemoveUnclesShield();
        }

        var startingEquipment = world.ComputeStartingItems();
        rom.InitialSram.SetStartingEquipment(startingEquipment, world);
        rom.SetBallNChainDungeon(0x02);
        rom.SetCapacityUpgradeFills(
            50, //item.value.BombUpgrade5
            50, //item.value.BombUpgrade10
            70, //item.value.ArrowUpgrade5
            70 //item.value.ArrowUpgrade10
        );

        // currently has to be after compass mode, as this will override compass mode.
        rom.SetClockMode("off"); //rom.timerMode

        rom.SetBlueClock(0); //item.value.BlueClock
        rom.SetRedClock(0); //item.value.RedClock
        rom.SetGreenClock(0); //item.value.GreenClock
        rom.InitialSram.SetStartingTimer(0); //rom.timerStart

        switch (config.Glitches) //rom.logicMode
        {
            //case GlitchesOption.HybridMajor:
            case GlitchesOption.Major:
            case GlitchesOption.NoLogic:
                rom.SetSwampWaterLevel(false);
                rom.SetPreAgahnimDarkWorldDeathInDungeon(false);
                rom.SetSaveAndQuitFromBossRoom(true);
                rom.SetWorldOnAgahnimDeath(false);
                rom.SetRandomizerSeedType("MajorGlitches");
                rom.SetWarningFlags(requiresMinorGlitches: true, requiresMajorGlitches: true);
                rom.SetAllowAccidentalMajorGlitch(true);
                rom.SetSQEGFix(false);
                rom.SetZeldaMirrorFix(false);
                break;
            case GlitchesOption.Overworld:
                rom.SetPreAgahnimDarkWorldDeathInDungeon(false);
                rom.SetSaveAndQuitFromBossRoom(true);
                rom.SetWorldOnAgahnimDeath(false);
                rom.SetRandomizerSeedType("OverworldGlitches");
                rom.SetWarningFlags(requiresMinorGlitches: true);
                rom.SetAllowAccidentalMajorGlitch(true);
                rom.SetSQEGFix(false);
                rom.SetZeldaMirrorFix(false);
                break;
            case GlitchesOption.None:
            default:
                rom.SetSaveAndQuitFromBossRoom(true);
                rom.SetWorldOnAgahnimDeath(true);
                rom.SetAllowAccidentalMajorGlitch(false);
                rom.SetSQEGFix(true);
                rom.SetZeldaMirrorFix(true);
                break;
        }

        bool triforceHUD = config.Goal is GoalOption.TriforceHunt or GoalOption.Trifecta //or GoalOption.GanonHunt
            || (config.TriforcePieces > 0);
        rom.EnableHudItemCounter(!triforceHUD && config.HudItemCounter /*|| config.Goal == GoalOption.Completionist*/); //rom.hudItemCounter

        if (config.CrystalsTower == "0")
            rom.InitialSram.PreOpenGanonsTower();

        rom.SetGameType("item");

        rom.SetMysteryMasking(false); //spoilers == "mystery"

        rom.SetPseudoBoots(config.PseudoBoots);

        rom.EnableFastRom(config.FastRom);

        if (config.TileRoomPattern != TileRoomPatternOption.Default)
        {
            var availablePatterns = YamlReader.LoadTileRoomPatterns();
            if (availablePatterns.Any())
            {
                var pattern = prng.GetRandomElement(availablePatterns);
                rom.SetTileRoomPattern(pattern);
            }
        }

        rom.WriteCredits();
        rom.WriteText();
        rom.WriteInitialSram();
        rom.SetTotalItemCount(world.PlacedItemCount);

        rom.SetSeedString("VT CSharp v32".PadRight(32));
        // FIXME: is this a useful hash? it should be the same for the same seed...
        rom.SetStartScreenHash([
            (byte)prng.GetRandomInt(0xFF),
            (byte)prng.GetRandomInt(0xFF),
            (byte)prng.GetRandomInt(0xFF),
            (byte)prng.GetRandomInt(0xFF),
            (byte)prng.GetRandomInt(0xFF)
        ]);
    }

    private static void SetProgressionText(World world, Rom rom, PRNG prng)
    {
        var config = world.Config;
        string language = world.WorldConfig.Language;
        var progressionHints = YamlReader.LoadHintsForProgression(language);
        var hints = new Dictionary<string, string>
        {
            { "blind_by_the_light", prng.GetRandomElement(YamlReader.LoadRandomDialogForBlind(language)) },
            { "kakariko_tavern_fisherman", prng.GetRandomElement(YamlReader.LoadRandomDialogForTavernMan(language)) },
            { "ganon_fall_in", prng.GetRandomElement(YamlReader.LoadRandomDialogForGanonFallIn(language)) },
            { "ganon_phase_3_alt", prng.GetRandomElement(YamlReader.LoadRandomDialogForGanonPhase3NoGoal(language)) },
            { "end_triforce", "{NOBORDER}\n" + prng.GetRandomElement(YamlReader.LoadRandomDialogForTriforce(language)) },
            { "sahasrahla_bring_courage", progressionHints["GreenPendantLocation"] },
            { "bomb_shop", progressionHints["Crystal56Location"] },
        };
        var locationByItem = world.GetLocationsOfType(VertexType.Item)
            .Where(v => v.Item != null)
            .OfType<Vertex>()
            .ToLookup(v => v.Item!.Name);

        // the boots reveal works in non-standard as well; except it is only on the sign east of Link's house.
        string uncleBootsText;
        if (config.RevealBootsLocation)
        {
            var bootsLocation = locationByItem["PegasusBoots"].FirstOrDefault();
            var bootsRevealHints = YamlReader.LoadHintsForBoots(language);

            if (bootsLocation is null)
                uncleBootsText = bootsRevealHints["NoBoots"];
            else if (bootsRevealHints.TryGetValue(bootsLocation.Name, out var bootsHint))
                uncleBootsText = bootsHint;
            else if (config.StartingEquipment.Contains("PegasusBoots"))
                uncleBootsText = bootsRevealHints["BootsStart"];
            else
                uncleBootsText = bootsRevealHints["BootsLocation"].Replace("{BOOTS}", bootsLocation.GetRegion(language));

            hints.Add("sign_east_of_links_house", uncleBootsText);
        }
        else
        {
            uncleBootsText = prng.GetRandomElement(YamlReader.LoadRandomDialogForUncle(language));
        }

        hints.Add("uncle_leaving_text", uncleBootsText);

        var silverArrowsUpgrade = locationByItem["SilverArrowUpgrade"].FirstOrDefault()
            ?? locationByItem["BowAndSilverArrows"].FirstOrDefault();
        var firstBow = locationByItem["ProgressiveBow"].FirstOrDefault();
        var secondBow = locationByItem["ProgressiveBowAlternate"].FirstOrDefault();

        string silversHint;
        string silversHintAlt;
        string silversLocation = "the void";
        string silversLocationAlt = "the void";
        if (firstBow != null && secondBow != null)
        {
            silversLocation = firstBow.GetRegion(language);
            silversLocationAlt = secondBow.GetRegion(language);

            if (silversLocation == "Ganon's Tower")
                silversLocation = "My Tower";
            if (silversLocationAlt == "Ganon's Tower")
                silversLocationAlt = "My Tower";

            string pattern = progressionHints["GanonSilversLocation"];
            silversHint = pattern;
            silversHintAlt = pattern.Replace("{SILVERS}", "{SILVERS_ALT}");
        }
        else if (silverArrowsUpgrade != null)
        {
            silversLocation = silverArrowsUpgrade.GetRegion(language);
            if (silversLocation == "Ganon's Tower")
                silversLocation = "My Tower";

            string pattern = progressionHints["GanonSilversLocation"];
            silversHint = pattern;
            silversHintAlt = silversHint;
        }
        else
        {
            silversHint = prng.GetRandomElement(YamlReader.LoadRandomDialogForGanonPhase3NoSilvers(language));
            silversHintAlt = silversHint;
        }

        hints.Add("ganon_phase_3_no_silvers", silversHint);
        hints.Add("ganon_phase_3_no_silvers_alt", silversHintAlt);

        if (config.Goal is GoalOption.TriforceHunt or GoalOption.Trifecta)
        {
            hints.Add("murahdahla", config.GoalRequiredCount == 1
                ? progressionHints["TriforceHandInSingular"]
                : progressionHints["TriforceHandInPlural"]);
        }
        if (int.Parse(config.CrystalsTower) < 7)
        {
            hints.Add("sign_ganons_tower", config.CrystalsTower == "1"
                ? progressionHints["TowerCrystalCountSingular"]
                : progressionHints["TowerCrystalCountPlural"]);
        }

        string ganonText = config.Goal switch
        {
            GoalOption.Pedestal => progressionHints["UnkillableGanonPedestal"],
            GoalOption.TriforceHunt => progressionHints["UnkillableGanonTriforceHunt"],
            _ => progressionHints["UnkillableGanonNoGoal"],
        };
        string? pyramidSign = config.Goal switch
        {
            // TODO: do we want/need a fast-ganon trifecta?
            GoalOption.Ganon or GoalOption.Trifecta => config.CrystalsGanon == "1"
                ? progressionHints["GanonCrystalCountSingular"]
                : progressionHints["GanonCrystalCountPlural"],
            GoalOption.FastGanon => config.CrystalsGanon == "1"
                ? progressionHints["FastGanonCrystalCountSingular"]
                : progressionHints["FastGanonCrystalCountPlural"],
            GoalOption.Dungeons => progressionHints["GanonAllDungeons"],
            GoalOption.Pedestal => progressionHints["GanonPedestal"],
            GoalOption.TriforceHunt => progressionHints["GanonTriforceHunt"],
            _ => null,
        };

        hints.Add("ganon_fall_in_alt", ganonText);
        if (!string.IsNullOrEmpty(pyramidSign))
            hints.Add("sign_ganon", pyramidSign);

        var greenPendant = locationByItem["PendantOfCourage"].FirstOrDefault();
        var crystal5 = locationByItem["Crystal5"].FirstOrDefault();
        var crystal6 = locationByItem["Crystal6"].FirstOrDefault();

        string greenPendantLocation = greenPendant?.GetRegion(language) ?? "Wrecked Ship";
        string crystal5Location = crystal5?.GetRegion(language) ?? "Tourian";
        string crystal6Location = crystal6?.GetRegion(language) ?? "Norfair";

        var replacements = new Dictionary<string, string>
        {
            { "{GREEN_PENDANT}", greenPendantLocation },
            { "{CRYSTAL5}", crystal5Location },
            { "{CRYSTAL6}", crystal6Location },
            { "{TOWER_COUNT}", $"{config.CrystalsTower}" },
            { "{GANON_COUNT}", $"{config.CrystalsGanon}" },
            { "{SILVERS}", silversLocation },
            { "{SILVERS_ALT}", silversLocationAlt },
            { "{TRIFORCE_PIECE_COUNT}", $"{config.GoalRequiredCount}" },
        };
        foreach (var (key, text) in hints)
            rom.SetText(key, ReplacePlaceholders(text, replacements));

        if (config.MapOnPickup) //rom.mapOnPickup
        {
            rom.SetMapRevealSahasrahla(greenPendant.GetMapReveal());
            rom.SetMapRevealBombShop((ushort)(crystal5.GetMapReveal() | crystal6.GetMapReveal()));
        }
    }

    private static string ReplacePlaceholders(string text, IDictionary<string, string> replacements)
    {
        foreach (var (placeholder, replacement) in replacements)
            text = text.Replace(placeholder, replacement);
        return text;
    }

    private static void SetHintText(Rom rom, WorldConfig config, Vertex location, IItem? item, GetItemTextOverride? getText)
    {
        var (hintKey, hintTextMap) = location.Name switch
        {
            "Master Sword Pedestal" => ("mastersword_pedestal_translated", YamlReader.LoadHintsForPedestal(config.Language)),
            "Ether Tablet" => ("tablet_ether_book", YamlReader.LoadHintsForEtherTablet(config.Language)),
            "Bombos Tablet" => ("tablet_bombos_book", YamlReader.LoadHintsForBombosTablet(config.Language)),
            _ => (null, null),
        };

        if (string.IsNullOrEmpty(hintKey))
            return;

        string hintText = getText?.Invoke(item, config) ?? "Don't waste\nyour time!";
        if (hintTextMap != null && item != null)
        {
            if (hintTextMap.TryGetValue(item.Name, out var specificItemText))
                hintText = specificItemText;
            else if (hintTextMap.TryGetValue("default", out var fallbackText))
                hintText = fallbackText;
        }
        rom.SetText(hintKey, hintText);
    }
    private static void SetHintText(World world, Rom rom, PRNG prng, GetItemTextOverride? getText)
    {
        var config = world.Config;
        if (!config.EnableHints)
        {
            rom.SetText("sign_north_of_links_house", "Randomizer v32\nDo you See Sharp?\n>    -veetorp");
            return;
        }

        string language = world.WorldConfig.Language;
        var tiles = prng.Shuffle([.. YamlReader.LoadHintLocations(language)]);
        var hints = new Queue<(string Location, string[] Items)>();
        var locationByItem = world.GetLocationsOfType(VertexType.Item)
            // FIXME: this returns meta items that have bytes, but aren't used as items (MM/TR entry)
            .Where(v => v.Item?.Bytes != null) // some locations have meta-items (which don't have bytes to write)
            .ToLookup(v => v.Item!.Name);
        var jokeHints = YamlReader.LoadJokeHints(language);
        var itemHints = YamlReader.LoadHintsForItems(language);
        var locationHints = YamlReader.LoadHintsForLocations(language);
        var locationTemplates = YamlReader.LoadHintTemplates(language);

        // keysanity: hint for GT big key
        if (config.RegionWildBigKeys)
            addLocationHint(locationByItem["BigKeyA2"].FirstOrDefault());

        // don't waste a hint on boots if we already revealed them
        if (!config.RevealBootsLocation)
            addLocationHint(locationByItem["PegasusBoots"].FirstOrDefault());

        // add 5 location hints
        var hintableLocations = prng.GetRandomElements(YamlReader.LoadHintableLocations(language), 5);
        foreach (var (location, subLocations) in hintableLocations)
        {
            // location is either an artificial location (group) that consists of many sub-locations;
            // or, when no sub-locations exist, the key is a specific single location
            if (subLocations is { Length: > 0 } locationsToCheck)
                addLocationHints(location, locationsToCheck.Select(l => world.GetLocation(l)?.Item!).Where(i => i is not null));
            else
                addLocationHint(world.GetLocation(location));
        }

        // add at most 4 item hints (progression items, including big keys if keysanity)
        // FIXME: what _are_ progression items at this point? the RandomAssumedFiller knows, but we don't.
        var progressionLocations = prng.GetRandomElements(locationByItem, Math.Min(4, tiles.Length - hints.Count));
        foreach (var itemAtLocations in progressionLocations)
        {
            // TODO: this biases hints to max one location per item; ie. no two different hints for different swords.
            addLocationHint(prng.GetRandomElement(itemAtLocations));
        }

        // add [remainingTiles/2, remainingTiles) item hints (anything)
        int remainingTiles = tiles.Length - hints.Count;
        var randomItemLocations = prng.GetRandomElements(locationByItem, prng.GetRandomInt(remainingTiles / 2, remainingTiles));
        foreach (var itemAtLocations in randomItemLocations)
        {
            // TODO: this biases hints to max one location per item; ie. no two different hints for different swords.
            addLocationHint(prng.GetRandomElement(itemAtLocations));
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            string text;
            if (hints.TryDequeue(out var locationItem))
            {
                var (location, items) = locationItem;
                // resolve internal names to hinted names
                items = [.. items.Select(item => prng.GetRandomElement(itemHints.GetValueOrDefault(item, [item])))];
                location = prng.GetRandomElement(locationHints.GetValueOrDefault(location, [location]));

                string singleOrLastItem = items.Last();
                bool isPlural = singleOrLastItem.Contains("{PLURAL}");
                bool noGlue = location.Contains("{NOGLUE}");
                var replacements = new Dictionary<string, string>
                {
                    { "{LOCATION}", location },
                    { "{ITEM}", singleOrLastItem },
                    { "{ITEMS}", string.Join(", ", items[..^1]) },
                    // special markers, not really a placeholder
                    { "{PLURAL}", "" },
                    { "{NOGLUE}", "" },
                };
                if (items.Length > 1)
                    text = locationTemplates[noGlue ? "multipleNoGlue" : "multiple"];
                else if (isPlural)
                    text = locationTemplates[noGlue ? "singleNoGlue" : "singlePlural"];
                else
                    text = locationTemplates[noGlue ? "singleNoGlue" : "single"];

                text = ReplacePlaceholders(text, replacements);
            }
            else
            {
                // fill the remainder with joke hints
                // joke hints are colored, because why not.
                text = "{C:GREEN}\n" + prng.GetRandomElement(jokeHints);
            }
            rom.SetText(tiles[i], text);
        }

        void addHints(string? location, params string[] items)
        {
            if (string.IsNullOrEmpty(location))
                return;
            // no usable items? leave the spot empty for a joke hint later.
            if (items.Length == 0)
                return;

            hints.Enqueue((location, items));
        }
        void addLocationHint(BaseVertex? location)
        {
            addLocationHints(location?.Name, location?.Item);
        }
        void addLocationHints(string? location, params IEnumerable<IItem?> items)
        {
            if (string.IsNullOrEmpty(location))
                return;
            var nnItems = items.Where(item => item != null).Select(item => item!).ToArray();
            if (nnItems.Length == 0)
                return;

            string[] itemHintList = [.. nnItems.Select(item => getText?.Invoke(item, world.WorldConfig) ?? prng.GetRandomElement(itemHints.GetValueOrDefault(item.Name, [item.Name])))];
            string locationHint = location;
            if (locationHints.TryGetValue(location, out var locs))
                locationHint = prng.GetRandomElement(locs);

            addHints(locationHint, itemHintList);
        }
    }

    private static void SetCreditsText(Rom rom, WorldConfig config, Vertex location, IItem? item, GetItemTextOverride? getText)
    {
        var (creditsKey, creditsTextMap) = location.Name switch
        {
            "Master Sword Pedestal" => ("pedestal", YamlReader.LoadCreditsForPedestal(config.Language)),
            "Link's Uncle" => ("house", YamlReader.LoadCreditsForUncle(config.Language)),
            "King Zora" => ("zora", YamlReader.LoadCreditsForZora(config.Language)),
            "Potion Shop Item" => ("witch", YamlReader.LoadCreditsForWitchHut(config.Language)),
            "Sick Kid Item" => ("kakariko2", YamlReader.LoadCreditsForSickKid(config.Language)),
            "Flute Spot" => ("grove", YamlReader.LoadCreditsForFluteSpot(config.Language)),
            _ => (null, null),
        };

        if (string.IsNullOrEmpty(creditsKey))
            return;

        string creditsText = getText?.Invoke(item, config) ?? "simply nothing";
        if (creditsTextMap != null && item != null)
        {
            if (creditsTextMap.TryGetValue(item.Name, out var specificItemText))
                creditsText = specificItemText;
            else if (creditsTextMap.TryGetValue("default", out var fallbackText))
                creditsText = fallbackText;
        }
        rom.SetCredit(creditsKey, creditsText);
    }
    private static void SetCreditsText(World world, Rom rom, PRNG prng)
    {
        string language = world.WorldConfig.Language;
        rom.SetCredit("bridge", prng.GetRandomElement(YamlReader.LoadCreditsForDMBridge(language)));
        rom.SetCredit("castle", prng.GetRandomElement(YamlReader.LoadCreditsForHyruleCastle(language)));
        rom.SetCredit("kakariko", prng.GetRandomElement(YamlReader.LoadCreditsForKakariko(language)));
        rom.SetCredit("lumberjacks", prng.GetRandomElement(YamlReader.LoadCreditsForLumberjacks(language)));
        rom.SetCredit("sanctuary", prng.GetRandomElement(YamlReader.LoadCreditsForSanctuary(language)));
        rom.SetCredit("smithy", prng.GetRandomElement(YamlReader.LoadCreditsForSmithy(language)));
        rom.SetCredit("well", prng.GetRandomElement(YamlReader.LoadCreditsForFairyWell(language)));
        rom.SetCredit("woods", prng.GetRandomElement(YamlReader.LoadCreditsForLostWoods(language)));
    }

    /// <summary>
    /// Set the ammo given for escape, based on available weapons
    /// </summary>
    /// <param name="rom">Rom to write data to</param>
    /// <param name="world"></param>
    private static void SetEscapeFills(World world, Rom rom)
    {
        var config = world.Config;

        var uncleItems = world.StartingItems.Clone();
        //uncleItems.setChecksForWorld(id);
        var uncleItem = world.GetLocation("Link's Uncle")?.Item;
        if (uncleItem != null)
            uncleItems.AddItem(uncleItem);

        // Add starting items if uncle doesn't have a weapon.  Temporarily disable ignoreCanKillEscapeThings for this check
        //bool ignoreCanKillEscapeThings = false; //ignoreCanKillEscapeThings
        //config['ignoreCanKillEscapeThings'] = false;
        //if (!uncleItems.canKillEscapeThings(this))
        //{
        //    uncleItems = uncleItems.merge(getPreCollectedItems());
        //}
        //config['ignoreCanKillEscapeThings'] = ignoreCanKillEscapeThings;

        // FIXME: that sword check doesn't cover everything.
        if (uncleItems.Has(world.GetItem("ProgressiveSword")) || uncleItems.Has(world.GetItem("Hammer")))
        {
            rom.SetEscapeFills();
            rom.SetUncleSpawnRefills(0, 0, 0);
            rom.SetZeldaSpawnRefills(0, 0, 0);
            rom.SetMantleSpawnRefills(0, 0, 0);
        }
        else if (
            uncleItems.Has(world.GetItem("FireRod"))
            || uncleItems.Has(world.GetItem("CaneOfSomaria"))
            || (uncleItems.Has(world.GetItem("CaneOfByrna"))) // && config('enemizer.enemyHealth', 'default') == 'default')
        )
        {
            rom.SetEscapeFills(refillMagic: true);
            rom.SetUncleSpawnRefills(
                0x80, //rom.EscapeRefills.Uncle.Magic
                0,
                0
            );
            rom.SetZeldaSpawnRefills(
                0x20, //rom.EscapeRefills.Zelda.Magic
                0,
                0
            );
            rom.SetMantleSpawnRefills(
                0x20, //rom.EscapeRefills.Mantle.Magic
                0,
                0
            );
            if (config.EscapeAssist) //rom.EscapeAssist
                rom.SetEscapeAssist(infiniteMagic: true);
        }
        // FIXME: that bow check (probably) doesn't cover everything.
        else if (uncleItems.Has(world.GetItem("Bow")))
        {
            rom.SetEscapeFills(refillArrows: true);
            rom.SetUncleSpawnRefills(
                0,
                0,
                7 //rom.EscapeRefills.Uncle.Arrows
            );
            rom.SetZeldaSpawnRefills(
                0,
                0,
                1 //rom.EscapeRefills.Zelda.Arrows
            );
            rom.SetMantleSpawnRefills(
                0,
                0,
                1 //rom.EscapeRefills.Mantle.Arrows
            );
            if (config.EscapeAssist) //rom.EscapeAssist
                rom.SetEscapeAssist(infiniteArrows: true);
        }
        else if (uncleItems.Has(world.GetItem("TenBombs")) || config.Glitches != GlitchesOption.NoLogic)
        {
            // TenBombs, or give player bombs if uncle was plando'd to not have a weapon.
            rom.SetEscapeFills(refillBombs: true);
            rom.SetUncleSpawnRefills(
                0,
                50, //rom.EscapeRefills.Uncle.Bombs
                0
            );
            rom.SetZeldaSpawnRefills(
                0,
                3, //rom.EscapeRefills.Zelda.Bombs
                0
            );
            rom.SetMantleSpawnRefills(
                0,
                3, //rom.EscapeRefills.Mantle.Bombs
                0
            );
            if (config.EscapeAssist) //rom.EscapeAssist
                rom.SetEscapeAssist(infiniteBombs: true);
        }
    }

    /// <summary>
    /// Writes prize pack locations and updates dig locations for rupee bow.
    /// </summary>
    /// <param name="rom">ROM to write data to</param>
    /// <param name="world"></param>
    private static void WritePrizePacksToRom(World world, Rom rom)
    {
        var config = world.Config;

        foreach (var prizePack in world.GetLocationsOfType(VertexType.PrizePack).OfType<Vertex>())
            rom.WriteSprite(prizePack);

        if (config.RomRupeeBow)
        {
            rom.SetOverworldDigPrizes([
                0xB2, // good bee
                .. Enumerable.Repeat((byte)0xD8, 3), // heart
                .. Enumerable.Repeat((byte)0xD8, 5), // heart
                .. Enumerable.Repeat((byte)0xD9, 5), // green rupee
                .. Enumerable.Repeat((byte)0xDA, 5), // blue rupee
                .. Enumerable.Repeat((byte)0xDB, 5), // red rupee
                .. Enumerable.Repeat((byte)0xDC, 5), // 1 bomb refill
                .. Enumerable.Repeat((byte)0xDD, 5), // 4 bomb refill
                .. Enumerable.Repeat((byte)0xDE, 5), // 8 bomb refill
                .. Enumerable.Repeat((byte)0xDF, 5), // small magic
                .. Enumerable.Repeat((byte)0xE0, 5), // large magic
                // replace arrow refills with rupees
                .. Enumerable.Repeat((byte)0xDA, 5), // blue rupee
                .. Enumerable.Repeat((byte)0xDB, 5), // red rupee
                .. Enumerable.Repeat((byte)0xE3, 5), // fairy
            ]);
        }
    }

    private static void WriteEntrancesToRom(World world, Rom rom)
    {
        var sourcesByTarget = world.Graph.GetVertices()
            .Where(v => v.World == world)
            .OfType<Vertex>()
            .SelectMany(v => v.Edges.Select(e => (Target: e.To, Source: v)))
            .ToLookup(k => k.Target, v => v.Source);

        var outletVertices = world.GetLocationsOfType(VertexType.Outlet).OfType<Vertex>().Where(v => v.OutletId.HasValue);
        var outletToMap = new Dictionary<int, int>();
        var outlets = new Dictionary<int, int>();
        foreach (var outletVertex in outletVertices)
        {
            var source = sourcesByTarget[outletVertex].FirstOrDefault(e => (e?.RoomId).HasValue);
            if (!(source?.RoomId).HasValue)
                throw new Exception($"Source outlet '{outletVertex.Name}' has no origin room with room id");

            // some rooms are reused (like 0x0112) for single entrance and don't have their own unique room id.
            // the rom will take care of that and send the player back to where they came from.
            // however: if we start in there, we need a place to go. this is what we write here.
            outlets.TryAdd(source!.RoomId!.Value, outletVertex.OutletId!.Value);
            outletToMap[outletVertex.OutletId.Value] = outletVertex.Map!.Value;
        }

        // we also tag exits as Entrance, so we have to look for the ones that have an EntranceId set.
        var entranceVertices = world.GetLocationsOfType(VertexType.Entrance).Where(v => v.EntranceId.HasValue);
        var entrances = new Dictionary<int, int>();
        foreach (var entranceVertex in entranceVertices)
        {
            if (entranceVertex.EntranceId < 0)
                continue;

            // entrances connect to a virtual entry node, which then connect to the inside
            var entranceTransition = entranceVertex.Edges.Select(e => e.To).FirstOrDefault();
            if (entranceTransition == null)
                throw new Exception($"No entrance connection for '{entranceVertex.Name}'");
            var target = entranceTransition.Edges.Select(e => e.To as Vertex).FirstOrDefault(e => (e?.InletId).HasValue);
            if (target == null)
                throw new Exception($"No entrance target for '{entranceVertex.Name}'");

            entrances.Add(entranceVertex.EntranceId!.Value, target.InletId!.Value);
        }

        var holeVertices = world.GetLocationsOfType(VertexType.Hole);
        var holes = new Dictionary<int, int>();
        foreach (var holeVertex in holeVertices)
        {
            if (holeVertex.EntranceIds == null)
                throw new Exception($"Hole '{holeVertex.Name}' has no entrance ids");
            var target = holeVertex.Edges.Select(e => e.To as Vertex).FirstOrDefault(e => (e?.InletId).HasValue);
            if (target == null)
                throw new Exception($"No entrance target for '{holeVertex.Name}'");

            foreach (var entranceId in holeVertex.EntranceIds ?? [])
                holes.Add(entranceId, target.InletId!.Value);
        }

        rom.WriteEntrances(outlets, entrances, holes, outletToMap);
    }


    /// <summary>
    /// Set enemy damage values based on configuration for world.
    /// </summary>
    /// <param name="world">world to pull config from</param>
    /// <param name="rom">rom to write data to</param>
    /// <param name="prng">prng to use for randomization</param>
    private static void WriteEnemyDamageToRom(World world, Rom rom, PRNG prng)
    {
        if (world.Config.EnemyDamage == EnemyDamageOption.Default)
            return;

        var damageBytes = rom.GetEnemyDamageTable();

        var updateTable = world.Config.EnemyDamage switch
        {
            EnemyDamageOption.Shuffled => prng.Shuffle(damageBytes.Select(v => v & 0x0F)).ToArray(),
            _ => Enumerable.Range(0, 0xF3).Select(_ => prng.GetRandomInt(0..9)).ToArray(),
        };

        for (int i = 0; i < 0xF3; i++)
        {
            damageBytes[i] = (byte)((damageBytes[i] & 0xF0) | (byte)updateTable[i]);
        }

        rom.SetEnemyDamageTable(damageBytes, prng);
    }

    // don't change the health value for those sprites
    private static readonly HashSet<int> _enemyHealthBlacklist =
    [
        0x70, // King Helmasaur fireball
        0x7A, // Agahnim
        0x7B, // Agahnim's balls
        0x89, // Mothula beam
        0xA3, // Kholdstare shell
        0xA4, // Falling ice
        0xBF, // Lightning
        0xCE, // Blind
    ];
    /// <summary>
    /// Set enemy health values based on configuration for world.
    /// </summary>
    /// <param name="world">world to pull config from</param>
    /// <param name="rom">rom to write data to</param>
    /// <param name="prng">prng to use for randomization</param>
    private static void WriteEnemyHealthToRom(World world, Rom rom, PRNG prng)
    {
        if (world.Config.EnemyHealth == EnemyHealthOption.Default)
            return;

        var healthBytes = rom.GetEnemyHealthTable();

        var lowest = world.Config.EnemyHealth switch
        {
            EnemyHealthOption.Expert => 4,
            EnemyHealthOption.Hard => 2,
            EnemyHealthOption.Medium => 2,
            _ => 1,
        };

        var highest = world.Config.EnemyHealth switch
        {
            EnemyHealthOption.Expert => 50,
            EnemyHealthOption.Hard => 25,
            EnemyHealthOption.Medium => 15,
            _ => 4,
        };
        for (int i = 0; i < 0xD3; i++)
        {
            if (healthBytes[i] == 0xFF || _enemyHealthBlacklist.Contains(i))
                continue;
            healthBytes[i] = (byte)prng.GetRandomInt(lowest..highest);
        }

        rom.SetEnemyHealthTable(healthBytes, lowest, highest, prng);
    }

    /// <summary>
    /// Write Room headers, and room data for all enemies in game.
    /// </summary>
    /// <param name="world">world to pull config from</param>
    /// <param name="rom">rom to write data to</param>
    private static void WriteEnemiesToRom(World world, Rom rom)
    {
        var enemies = world.GetLocationsOfType(VertexType.Mob);
        var enemyRooms = enemies.ToLookup(enemy => enemy.RoomId);

        // remove the maiden if blind isn't in thieves town.
        var thievesTownBoss = world.GetLocation("Thieves' Town - Boss Room - Blind Active");
        var blindBossEdge = thievesTownBoss.Edges.FirstOrDefault(e => e.Condition.Item?.Name is "DefeatBlind" or "DarkDefeatBlind");
        bool skipTheMaiden = blindBossEdge == null;

        var outputOffsets = new ushort[0x140];
        // empty room ;)
        List<byte> outputBytes = [0x00, 0xFF];
        for (int i = 0; i < outputOffsets.Length; i++)
        {
            if (!enemyRooms[i].Any())
            {
                outputOffsets[i] = 0x0000;
                continue;
            }
            outputOffsets[i] = (ushort)outputBytes.Count;
            // reconfigure OAM allocation to work with overlapping layers
            // TODO: room 0x86 is empty (so we don't have a data file), but it has the flag set in vanilla
            byte roomOAM = enemyRooms[i].Select(r => r.RoomOAM.GetValueOrDefault()).Concat([(byte)0x00]).Max();
            outputBytes.Add(roomOAM);
            // prioritize enemies (like bosses) since they might need require OAM slots
            foreach (var enemy in enemyRooms[i].OrderByDescending(e => e.Sprite!.Priority))
            {
                var sprite = enemy.Sprite!;
                if (sprite.Name == "BlindMaiden" && skipTheMaiden)
                {
                    rom.BlindIsNotInThievesTown();
                    continue;
                }
                outputBytes.Add((byte)(((sprite.SubType & 0x18) << 2)
                    | (enemy.Position!.Z.GetValueOrDefault() << 7)
                    | enemy.Position.Y));
                byte overlordFlag = (byte)(sprite.Flags.HasFlag(YamlSpriteFlags.Overlord) ? 0b11111 << 5 : 0x00);
                outputBytes.Add((byte)(((sprite.SubType & 0x07) << 5)
                    | overlordFlag
                    | enemy.Position.X));
                outputBytes.Add(sprite.Id);
                if (enemy.Item is Item enemyDrop)
                {
                    // TODO: update this when we can place any item
                    outputBytes.Add((byte)(enemyDrop.Type == ItemType.BigKey ? 0xFD : 0xFE));
                    outputBytes.Add(0x00);
                    outputBytes.Add(0xE4);
                }
            }
            outputBytes.Add(0xFF);
        }

        var headerChanges = new RoomHeaderPatches();
        var roomChanges = new RoomObjectPatches();
        var bossData = YamlReader.LoadBossSprites();
        foreach (var boss in world.GetLocationsOfType(VertexType.Boss))
        {
            int roomId = boss.RoomId!.Value;
            var vanillaBoss = BossShuffler.GetVanillaBoss(boss);
            // FIXME: abusing the allow list to remember the boss item is probably a bad idea...
            string bossItem = boss.Allow!.Single();
            // no need to modify the room if it's the vanilla boss.
            if (bossItem == vanillaBoss)
                continue;

            var bossSprites = bossData[bossItem];
            foreach (var bossSprite in bossSprites)
            {
                if (bossSprite.LowerLayer is { } lowerLayerPatches)
                    roomChanges[roomId].LowerLayer = lowerLayerPatches.SelectMany(patch => makeObjectLayerValue(patch, bossSprite, boss)).ToArray();

                if (bossSprite.BG2Prop is byte bg2Prop)
                {
#if DEBUG
                    if (headerChanges[roomId].Background2Properties.HasValue && headerChanges[roomId].Background2Properties != bg2Prop)
                        throw new Exception($"Found second {bg2Prop} BG2PROP change for {bossItem} ({headerChanges[roomId].Background2Properties}) currently set), only one is supported.");
#endif
                    headerChanges[roomId].Background2Properties = bg2Prop;
                }
                if (bossSprite.Blkset is byte blkset)
                {
#if DEBUG
                    if (headerChanges[roomId].BlockSet.HasValue && headerChanges[roomId].BlockSet != blkset)
                        throw new Exception($"Found second {blkset} BLKSET change for {bossItem} ({headerChanges[roomId].BlockSet}) currently set), only one is supported.");
#endif
                    headerChanges[roomId].BlockSet = blkset;
                }
                if (bossSprite.Floor1 is byte floor1)
                {
#if DEBUG
                    if (roomChanges[roomId].Floor1.HasValue && roomChanges[roomId].Floor1 != floor1)
                        throw new Exception($"Found second {floor1} FLOOR1 change for {bossItem} ({roomChanges[roomId].Floor1}) currently set), only one is supported.");
#endif
                    roomChanges[roomId].Floor1 = floor1;
                }
                if (bossSprite.Floor2 is byte floor2)
                {
#if DEBUG
                    if (roomChanges[roomId].Floor2.HasValue && roomChanges[roomId].Floor2 != floor2)
                        throw new Exception($"Found second {floor2} FLOOR2 change for {bossItem} ({roomChanges[roomId].Floor2}) currently set), only one is supported.");
#endif
                    roomChanges[roomId].Floor2 = floor2;
                }
            }

            // we might have to clear the lower layer (if it contains things like Kholdstare's/Trinexx' shell).
            if (vanillaBoss is "DefeatTrinexx" or "DefeatKholdstare" && (!roomChanges.TryGet(roomId, out var roomPatch) || roomPatch.LowerLayer == null))
                roomChanges[roomId].LowerLayer = [];
        }
        rom.WriteUnderworldRoomsChanges(roomChanges);
        static byte[] makeObjectLayerValue(YamlRoomObjectPatch roomObject, YamlBossSprite sprite, Vertex boss)
        {
            var position = boss.RoomOffset + sprite.Position;
            // this is a map16 position (so the actual location is *16)...
            position *= 16;
            // ...but objects are laid out differently, on an 8-based grid.
            position /= 8;
            // ...which is why we need a little offset to fit Kholdstare into the middle of the shell.
            position += roomObject;
            ushort objectId = roomObject.ObjectId;
            return [
                (byte)(((position.X << 2) & 0xFC) | (objectId & 0x3)),
                (byte)(((position.Y << 2) & 0xFC) | ((objectId >> 2) & 0x3)),
                (byte)(0xF8 | ((objectId >> 4) & 0xF)),
            ];
        }

        rom.WriteUnderworldEnemies([.. outputBytes], outputOffsets, world.SpriteSheets.Underworld, headerChanges);

        // Overworld
        List<ushort>[] owPointerOffsets = [[], [], []];
        List<ushort> owOutputOffsets = [];
        // empty map ;)
        outputBytes = [0xFF];
        var enemyMaps = enemies.ToLookup(enemy => enemy.Map);
        var mapsBytes = new Dictionary<ushort, byte[]> { [0x0000] = [.. outputBytes] };
        for (int state = 0; state <= 2; state++)
        {
            // Pointer to pointer table
            owPointerOffsets[state].Add((ushort)(owOutputOffsets.Count * 2));

            for (int i = 0; i < 0x90; i++)
            {
                // rain state only has light world in the table; the game doesn't expect you in dark world before rescuing Zelda.
                if (state == 0 && i > 0x3f)
                    break;

                List<byte> outputMap = [];
                foreach (var enemy in enemyMaps[i])
                {
                    if (enemy.State != null && !enemy.State.Contains(state))
                        continue;

                    outputMap.Add((byte)enemy.Position!.Y);
                    outputMap.Add((byte)enemy.Position.X);
                    byte enemyId = enemy.Sprite!.Id;
                    if (enemy.Sprite.Flags.HasFlag(YamlSpriteFlags.Overlord))
                        enemyId += 0xF2;
                    outputMap.Add(enemyId);
                }

                if (outputMap.Count == 0)
                {
                    owOutputOffsets.Add(0x0000);
                    continue;
                }

                outputMap.Add(0xFF);

                ushort possibleRepeat = mapsBytes.FirstOrDefault(k => k.Value.SequenceEqual(outputMap)).Key;
                if (possibleRepeat != 0)
                {
                    owOutputOffsets.Add(possibleRepeat);
                    continue;
                }

                owOutputOffsets.Add((ushort)outputBytes.Count);
                mapsBytes[(ushort)outputBytes.Count] = [.. outputMap];
                outputBytes.AddRange(outputMap);
            }
        }

        rom.WriteOverworldEnemies([.. outputBytes], [.. owOutputOffsets], owPointerOffsets, world.SpriteSheets.Overworld, world.SpriteSheets.Special);
        // write new sheet sets
        rom.WriteSpriteSheetSets(world.SpriteSheets.Sets);

        rom.WriteSpriteData(world.Sprites.Values);
    }
}

public sealed class RoomHeaderPatches
{
    private readonly ConcurrentDictionary<int /* RoomId */, RoomHeaderPatch> _patches = [];
    public RoomHeaderPatch this[int roomId] => _patches.GetOrAdd(roomId, _ => new());
    public bool TryGet(int roomId, [MaybeNullWhen(returnValue: false)] out RoomHeaderPatch roomHeaderPatch) => _patches.TryGetValue(roomId, out roomHeaderPatch);
    public IEnumerable<int> Rooms => _patches.Keys;
}
public sealed class RoomHeaderPatch
{
    public byte? Background2Properties { get; set; }
    public byte? BlockSet { get; set; }
    public byte? BackgroundMove { get; set; }
    public byte? Effect1 { get; set; }
    public byte? Effect2 { get; set; }
}

public sealed class RoomObjectPatches
{
    private readonly ConcurrentDictionary<int /* RoomId */, RoomObjectPatch> _patches = [];
    public RoomObjectPatch this[int roomId] => _patches.GetOrAdd(roomId, _ => new());
    public bool TryGet(int roomId, [MaybeNullWhen(returnValue: false)] out RoomObjectPatch roomHeaderPatch) => _patches.TryGetValue(roomId, out roomHeaderPatch);
    public IEnumerable<int> Rooms => _patches.Keys;
}
public sealed class RoomObjectPatch
{
    public byte? Layout { get; set; }
    public byte? Floor1 { get; set; }
    public byte? Floor2 { get; set; }
    public byte[]? UpperLayer { get; set; }
    public byte[]? LowerLayer { get; set; }
    public byte[]? PriorityLayer { get; set; }
}
