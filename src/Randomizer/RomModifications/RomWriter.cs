namespace Randomizer.RomModifications;

using Randomizer.Graph;

public static class RomWriter
{
    public static void Write(Randomizer randomizer, FileInfo baseRom, FileInfo? baseBPS, DirectoryInfo outputDirectory)
    {
        foreach (var (i, world) in randomizer.Worlds.Select((world, index) => (index + 1, world)))
            WriteForWorld(world, baseRom, baseBPS, outputDirectory, randomizer.PRNG, randomizer.Worlds.Length > 1 ? $"_W{i}" : null);
    }

    private static readonly HeartColorOption[] _heartColorOptions = [HeartColorOption.Blue, HeartColorOption.Green, HeartColorOption.Yellow, HeartColorOption.Red];
    public static void WriteForWorld(World world, FileInfo baseRom, FileInfo? baseBPS, DirectoryInfo outputDirectory, PRNG prng, string? worldSuffix = null)
    {
        using var rom = new Rom(baseRom.FullName);
        // TODO: check hash? do we need that?

        // assume we either have a vanilla rom and a BPS, or an already pre-patched base rom.
        if (baseBPS != null)
        {
            rom.Resize();
            rom.ApplyBasePatch(baseBPS);
        }

        var heartColor = world.Config.HeartColor; //option('heartcolor')
        if (heartColor == HeartColorOption.Random)
                heartColor = prng.GetRandomElement(_heartColorOptions);
            rom.SetHeartColors(heartColor);
        rom.SetHeartBeepSpeed(world.Config.HeartBeepSpeed); //option('heartbeep')

        rom.SetQuickSwap(world.Config.QuickSwap); //option('quickswap')

        WriteWorld(world, rom, prng);

        rom.MuteMusic(world.Config.NoMusic); //option('no-music')
        rom.SetMenuSpeed(world.Config.MenuSpeed); //option('menu-speed')

        // TODO: patch in the sprite
        // TODO: tournament mode

        rom.UpdateChecksum();

        outputDirectory.Create();
        string outputFile = Path.Combine(
            outputDirectory.FullName,
            $"alttpr_{world.Config.Glitches}_{world.Config.State}_{world.Config.Goal}_{prng.Seed:x08}{worldSuffix}.sfc");
        rom.Save(outputFile);
    }
    private static void WriteWorld(World world, Rom rom, PRNG prng)
    {
        var config = world.Config;

        if (!false) //config("multiworld", false))
        {
            var nothing = world.GetItem("Nothing");
            foreach (var location in world.GetLocationsOfType(VertexType.Item))
            {
                var itemToWrite = location.Item ?? nothing;

                rom.WriteItem(location, itemToWrite);
                rom.WriteCreditsText(location, itemToWrite);
                rom.WriteDungeonMusic(location, itemToWrite, prng);
                rom.WriteHintText(location, itemToWrite);
                rom.WriteLocationSpecificData(location, itemToWrite);
            }
        }

        if (config.State == StateOption.Standard)
            SetEscapeFills(world, rom);

        rom.SetGoalRequiredCount(config.GoalRequiredCount); //item.Goal.Required
        rom.SetGoalIcon(config.GoalIcon); //item.Goal.Icon

        // Set item functionality settings
        rom.SetCaneOfByrnaSpikeCaveUsage();
        rom.SetCapeSpikeCaveUsage();
        rom.SetByrnaCaveSpikeDamage(0x08);
        rom.SetCaneOfByrnaMagicPerCycle();

        rom.SetCapeRegularMagicUsage(
            config.CapeMagicUsageNormal, //rom.CapeMagicUsage.Normal
            config.CapeMagicUsageHalf, //rom.CapeMagicUsage.Half
            config.CapeMagicUsageQuarter //rom.CapeMagicUsage.Quarter
        );
        rom.SetCaneOfByrnaInvulnerability(config.CaneOfByrnaInvulnerability); //rom.CaneOfByrnaInvulnerability
        rom.SetPowderedSpriteFairyPrize(config.PowderedSpriteFairyPrize); //rom.PowderedSpriteFairyPrize
        rom.SetBottleFills(
            config.BottleFillHealth, //rom.BottleFill.Health
            config.BottleFillMagic //rom.BottleFill.Magic
        );
        rom.SetCatchableFairies(config.CatchableFairies); //rom.CatchableFairies
        rom.SetCatchableBees(config.CatchableBees); //rom.CatchableBees
        rom.SetStunItems(config.StunItemsHookshot, config.StunItemsBoomerang); //rom.StunItems
        rom.SetSilversOnlyAtGanon(config.SilversOnlyAtGanon); //rom.SilversOnlyAtGanon

        rom.SetRupoorValue(0); //item.value.Rupoor

        rom.SetGanonAgahnimRng(config.GanonAgahnimRNG); //rom.GanonAgRNG

        rom.SetTowerCrystalRequirement(config.CrystalsTower);
        rom.SetGanonCrystalRequirement(config.CrystalsGanon);

        // testing features
        rom.SetGenericKeys(config.GenericKeys); //rom.genericKeys
        //rom.SetupCustomShops(getShops());
        rom.SetRupeeArrow(config.RomRupeeBow); //rom.rupeeBow
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

            default:
                rom.SetGanonInvincible("crystals_only");
                break;
        }

        SetProgressionText(world, rom);

        rom.SetMapMode(config.MapOnPickup); //rom.mapOnPickup
        rom.SetCompassMode(config.CompassCounter); //rom.dungeonCount
        rom.SetCompassCountTotals();
        rom.SetFreeItemTextMode(); //rom.freeItemText
        rom.SetFreeItemMenu(); //rom.freeItemMenu
        rom.SetDiggingGameRng((byte)prng.GetRandomInt(1, 30));

        rom.WriteRNGBlock(() => (byte)prng.GetRandomInt(0, 0x100));

        WritePrizePacksToRom(world, rom);

        rom.SetPyramidFairyChests(config.Weapon != WeaponOption.Vanilla); //region.swordsInPool
        rom.SetSmithyQuickItemGive(config.Weapon != WeaponOption.Vanilla); //region.swordsInPool

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

        bool triforceHUD = config.Goal is GoalOption.TriforceHunt //or GoalOption.GanonHunt
            || (config.TriforcePieces > 0);
        rom.EnableHudItemCounter(!triforceHUD && config.HudItemCounter /*|| config.Goal == GoalOption.Completionist*/); //rom.hudItemCounter

        if (config.CrystalsTower == 0)
            rom.InitialSram.PreOpenGanonsTower();

        rom.SetGameType("item");

        rom.SetMysteryMasking(false); //spoilers == "mystery"

        rom.SetPseudoBoots(config.PseudoBoots); //pseudoboots

        rom.EnableFastRom(config.FastRom); //fastrom

        rom.WriteCredits();
        rom.WriteText();
        rom.WriteInitialSram();
        //rom.SetTotalItemCount(getTotalItemCount());

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

    private static void SetProgressionText(World world, Rom rom)
    {
        var locationByPrize = world.GetLocationsOfType(VertexType.Item)
            .Where(v => v.SubType == VertexType.Prize && v.Item != null)
            .ToDictionary(v => v.Item!.Name);

        var greenPendant = locationByPrize.GetValueOrDefault("PendantOfCourage", null!);
        var crystal5 = locationByPrize.GetValueOrDefault("Crystal5", null!);
        var crystal6 = locationByPrize.GetValueOrDefault("Crystal6", null!);

        // TODO: this only works because of our naming convention "Region - Location"; we probably want something more stable.
        string greenPendantLocation = greenPendant?.Name.Split(" - ").FirstOrDefault() ?? "Wrecked Ship";
        string crystal5Location = crystal5?.Name.Split(" - ").FirstOrDefault() ?? "Tourian";
        string crystal6Location = crystal6?.Name.Split(" - ").FirstOrDefault() ?? "Norfair";

        rom.SetText("sahasrahla_bring_courage", $"Want something\nfor free? Go\nearn the green\npendant in\n{greenPendantLocation}\nand I'll give\nyou something.");
        rom.SetText("bomb_shop", $"bring me the\ncrystals from\n{crystal5Location}\nand\n{crystal6Location}\nso I can make\na big bomb!");
        if (world.Config.MapOnPickup) //rom.mapOnPickup
        {
            rom.SetMapRevealSahasrahla(greenPendant.GetMapReveal());
            rom.SetMapRevealBombShop((ushort)(crystal5.GetMapReveal() | crystal6.GetMapReveal()));
        }

        if (world.Config.Goal == GoalOption.TriforceHunt)
        {
            rom.SetText("murahdahla",
$@"Hello @. I
am Murahdahla, brother of
Sahasrahla and Aginah. Behold the power of
invisibility.



… … …

Wait! you can see me? I knew I should have
hidden in  a hollow tree. If you bring
{world.Config.GoalRequiredCount} triforce pieces, I can reassemble it.");
        }
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
    /// This is a quick hack to get prizes shuffled, will adjust later when we model sprites.
    /// this now also handles prize pull trees.
    /// </summary>
    /// <param name="rom">ROM to write data to</param>
    /// <param name="world"></param>
    private static void WritePrizePacksToRom(World world, Rom rom)
    {
#if PrizePacksWork
        var config = world.Config;

        var emptyDrops = getEmptyDropSlots();
        var dropPool = getDropsPool();

        for (int i = 0; i < emptyDrops.Count; i++)
            emptyDrops[i].setDrop(dropPool[i]);

        byte[] dropBytes = getAllDrops().Select(prize => prize.getDrop().getBytes()[0]).ToArray();

        // hard+ does not allow fairies/full magics
        if (config('rom.NoFarieDrops', false))
            dropBytes = str_replace([0xE0, 0xE3], [0xDF, 0xD8], dropBytes);

        if (config.RomRupeeBow)
        {
            dropBytes = str_replace([0xE1, 0xE2], [0xDA, 0xDB], dropBytes);
            rom.SetOverworldDigPrizes([
                0xB2,
                0xD8,
                0xD8,
                0xD8,
                0xD8,
                0xD8,
                0xD8,
                0xD8,
                0xD8,
                0xD9,
                0xD9,
                0xD9,
                0xD9,
                0xD9,
                0xDA,
                0xDA,
                0xDA,
                0xDA,
                0xDA,
                0xDB,
                0xDB,
                0xDB,
                0xDB,
                0xDB,
                0xDC,
                0xDC,
                0xDC,
                0xDC,
                0xDC,
                0xDD,
                0xDD,
                0xDD,
                0xDD,
                0xDD,
                0xDE,
                0xDE,
                0xDE,
                0xDE,
                0xDE,
                0xDF,
                0xDF,
                0xDF,
                0xDF,
                0xDF,
                0xE0,
                0xE0,
                0xE0,
                0xE0,
                0xE0,
                0xDA,
                0xDA,
                0xDA,
                0xDA,
                0xDA,
                0xDB,
                0xDB,
                0xDB,
                0xDB,
                0xDB,
                0xE3,
                0xE3,
                0xE3,
                0xE3,
                0xE3,
            ]);
        }

        // write to prize packs
        rom.SetPrizePacks(dropBytes[..56]);

        // write to trees
        rom.SetPullTreePrizes(dropBytes[56], dropBytes[57], dropBytes[58]);

        // write to prize crab
        rom.SetRupeeCrabPrizes(dropBytes[59], dropBytes[60]);

        // write to stunned
        rom.SetStunnedSpritePrize(dropBytes[61]);

        // write to saved fish
        rom.SetFishSavePrize(dropBytes[62]);
#endif
    }
}
