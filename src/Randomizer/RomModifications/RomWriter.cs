namespace Randomizer.RomModifications;

using Randomizer.Graph;

public static class RomWriter
{
    public static void Write(Randomizer randomizer, FileInfo vanillaRom, FileInfo baseBPS, DirectoryInfo outputDirectory)
    {
        foreach (var world in randomizer.Worlds)
            WriteForWorld(world, vanillaRom, baseBPS, outputDirectory, randomizer.PRNG);
    }

    private static readonly string[] _heartColorOptions = ["blue", "green", "yellow", "red"];
    public static void WriteForWorld(World world, FileInfo vanillaRom, FileInfo baseBPS, DirectoryInfo outputDirectory, PRNG prng)
    {
        var rom = new Rom(vanillaRom.FullName);
        // TODO: check hash? do we need that?

        rom.Resize();
        rom.ApplyBasePatch(baseBPS);

        string? heartColor = null; //option('heartcolor')
        if (!string.IsNullOrWhiteSpace(heartColor))
        {
            if (heartColor == "random")
                heartColor = prng.GetRandomElement(_heartColorOptions);
            rom.SetHeartColors(heartColor);
        }

        string? heartBeep = null; //option('heartbeep')
        if (!string.IsNullOrWhiteSpace(heartBeep))
            rom.SetHeartBeepSpeed(heartBeep);

        bool? quickSwap = null; //option('quickswap')
        if (quickSwap.HasValue)
            rom.SetQuickSwap(quickSwap.Value);

        WriteWorld(world, rom, prng);

        bool? noMusic = null; //option('no-music')
        rom.MuteMusic(noMusic.GetValueOrDefault());
        rom.SetMenuSpeed("normal"); //option('menu-speed')

        // TODO: patch in the sprite
        // TODO: tournament mode

        rom.UpdateChecksum();

        outputDirectory.Create();
        string outputFile = Path.Combine(
            outputDirectory.FullName,
            $"alttpr_{world.Config.Glitches}_{world.Config.State}_{world.Config.Goal}_{prng.Seed:x08}.sfc");
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
            }
        }

        if (config.State == StateOption.Standard)
            SetEscapeFills(world, rom);

        rom.SetGoalRequiredCount(0); //item.Goal.Required
        rom.SetGoalIcon("triforce"); //item.Goal.Icon

        // Set item functionality settings
        rom.SetCaneOfByrnaSpikeCaveUsage();
        rom.SetCapeSpikeCaveUsage();
        rom.SetByrnaCaveSpikeDamage(0x08);
        rom.SetCaneOfByrnaMagicPerCycle();

        rom.SetCapeRegularMagicUsage(
            0x04, //rom.CapeMagicUsage.Normal
            0x08, //rom.CapeMagicUsage.Half
            0x10 //rom.CapeMagicUsage.Quarter
        );
        rom.SetCaneOfByrnaInvulnerability(true); //rom.CaneOfByrnaInvulnerability
        rom.SetPowderedSpriteFairyPrize(0xE3); //rom.PowderedSpriteFairyPrize
        rom.SetBottleFills(
            0xA0,  //rom.BottleFill.Health
            0x80 //rom.BottleFill.Magic
        );
        rom.SetCatchableFairies(true); //rom.CatchableFairies
        rom.SetCatchableBees(true); //rom.CatchableBees
        rom.SetStunItems(true, true); //rom.StunItems
        rom.SetSilversOnlyAtGanon(false); //rom.SilversOnlyAtGanon

        rom.SetRupoorValue(0); //item.value.Rupoor

        rom.SetGanonAgahnimRng("table"); //rom.GanonAgRNG

        rom.SetTowerCrystalRequirement(config.CrystalsTower);
        rom.SetGanonCrystalRequirement(config.CrystalsGanon);

        // testing features
        rom.SetGenericKeys(false); //rom.genericKeys
        //rom.SetupCustomShops(getShops());
        rom.SetRupeeArrow(false); //rom.rupeeBow
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

        rom.SetSilversEquip("collection");
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

        if (false) //rom.mapOnPickup
        {
            var locationByPrize = world.Graph.GetVertices()
                .Where(v => v.Type == VertexType.Item && v.SubType == VertexType.Prize && v.Item != null)
                .ToDictionary(v => v.Item!.Name);

            var greenPendant = locationByPrize.GetValueOrDefault("PendantOfCourage", null!).GetMapReveal();
            rom.SetMapRevealSahasrahla(greenPendant);

            var crystal5 = locationByPrize.GetValueOrDefault("Crystal5", null!).GetMapReveal();
            var crystal6 = locationByPrize.GetValueOrDefault("Crystal6", null!).GetMapReveal();
            rom.SetMapRevealBombShop((ushort)(crystal5 | crystal6));
        }

        rom.SetMapMode(false); //rom.mapOnPickup
        rom.SetCompassMode("off"); //rom.dungeonCount
        rom.SetCompassCountTotals();
        rom.SetFreeItemTextMode(); //rom.freeItemText
        rom.SetFreeItemMenu(); //rom.freeItemMenu
        rom.SetDiggingGameRng((byte)prng.GetRandomInt(1, 30));

        rom.WriteRNGBlock(() => (byte)prng.GetRandomInt(0, 0x100));

        WritePrizePacksToRom(world, rom);

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

        var linksUncleItem = world.GetLocation("Link's Uncle").Item;
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

        //bool triforce_hud = config.Goal is GoalOption.TriforceHunt or GoalOption.GanonHunt
        //    || (config("item.Goal.Required", 0) > 0);
        //rom.EnableHudItemCounter(triforce_hud ? false : config("rom.hudItemCounter", config("goal", "ganon") == "completionist"));
        rom.EnableHudItemCounter(false);

        if (config.CrystalsTower == 0)
            rom.InitialSram.PreOpenGanonsTower();

        rom.SetGameType("item");

        rom.SetMysteryMasking(false); //spoilers == "mystery"

        rom.SetPseudoBoots(false); //pseudoboots

        rom.EnableFastRom(true); //fastrom

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

    /// <summary>
    /// Set the ammo given for escape, based on available weapons
    /// </summary>
    /// <param name="rom">Rom to write data to</param>
    /// <param name="world"></param>
    private static void SetEscapeFills(World world, Rom rom)
    {
        var config = world.Config;

        var uncle_items = world.StartingItems.Clone();
        //uncle_items.setChecksForWorld(id);
        var uncleItem = world.GetLocation("Link's Uncle").Item;
        if (uncleItem != null)
            uncle_items.AddItem(uncleItem);

        // Add starting items if uncle doesn't have a weapon.  Temporarily disable ignoreCanKillEscapeThings for this check
        //bool ignoreCanKillEscapeThings = false; //ignoreCanKillEscapeThings
        //config['ignoreCanKillEscapeThings'] = false;
        //if (!uncle_items.canKillEscapeThings(this))
        //{
        //    uncle_items = uncle_items.merge(getPreCollectedItems());
        //}
        //config['ignoreCanKillEscapeThings'] = ignoreCanKillEscapeThings;

        // FIXME: that sword check doesn't cover everything.
        if (uncle_items.Has(world.GetItem("ProgressiveSword")) || uncle_items.Has(world.GetItem("Hammer")))
        {
            rom.SetEscapeFills();
            rom.SetUncleSpawnRefills(0, 0, 0);
            rom.SetZeldaSpawnRefills(0, 0, 0);
            rom.SetMantleSpawnRefills(0, 0, 0);
        }
        else if (
            uncle_items.Has(world.GetItem("FireRod"))
            || uncle_items.Has(world.GetItem("CaneOfSomaria"))
            || (uncle_items.Has(world.GetItem("CaneOfByrna"))) // && config('enemizer.enemyHealth', 'default') == 'default')
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
            if (false) //rom.EscapeAssist
                rom.SetEscapeAssist(infiniteMagic: true);
        }
        // FIXME: that bow check (probably) doesn't cover everything.
        else if (uncle_items.Has(world.GetItem("Bow")))
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
            if (false) //rom.EscapeAssist
                rom.SetEscapeAssist(infiniteArrows: true);
        }
        else if (uncle_items.Has(world.GetItem("TenBombs"))) // || config('logic') != 'NoLogic')
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
            if (false) //rom.EscapeAssist
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
        var drop_pool = getDropsPool();

        for (int i = 0; i < emptyDrops.Count; i++)
            emptyDrops[i].setDrop(drop_pool[i]);

        byte[] drop_bytes = getAllDrops().Select(prize => prize.getDrop().getBytes()[0]).ToArray();

        // hard+ does not allow fairies/full magics
        if (config('rom.NoFarieDrops', false))
            drop_bytes = str_replace([0xE0, 0xE3], [0xDF, 0xD8], drop_bytes);

        if (config.RomRupeeBow)
        {
            drop_bytes = str_replace([0xE1, 0xE2], [0xDA, 0xDB], drop_bytes);
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
        rom.SetPrizePacks(drop_bytes[..56]);

        // write to trees
        rom.SetPullTreePrizes(drop_bytes[56], drop_bytes[57], drop_bytes[58]);

        // write to prize crab
        rom.SetRupeeCrabPrizes(drop_bytes[59], drop_bytes[60]);

        // write to stunned
        rom.SetStunnedSpritePrize(drop_bytes[61]);

        // write to saved fish
        rom.SetFishSavePrize(drop_bytes[62]);
#endif
    }
}
