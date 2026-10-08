namespace Randomizer.Games.Alttp;

using Randomizer.Games.Metadata;

public sealed record class SpriteProperties
{
    public required string Name { get; init; }

    public required byte ID { get; init; }

    /// <summary>
    /// When <see langword="false"/>, this indicates a sprite is a variant that should
    /// not provide any randomization for its ID other than the delegates it provides.
    /// </summary>
    public bool OwnsId { get; init; } = true;

    /// <summary>
    /// Gets or sets the default HP of this sprite.
    /// </summary>
    [DisassemblyName(FileName = "bank_0D.asm", SymbolName = "SpriteData_Health", Address = 0x0DB173)]
    [ValueRange(0, 255)]
    public byte Property_HP { get; set; }
    public int[]? HpAddresses { get; init; } = null;



    [PackedBitfield(nameof(IsHarmless))]
    [PackedBitfield(nameof(KillWhenOffscreenDuringTileChecks))]
    [PackedBitfield(nameof(UseTileHitbox68))]
    [PackedBitfield(nameof(OamAllocation))]
    [DisassemblyName(FileName = "bank_0D.asm", SymbolName = "SpriteData_OAMHarm", Address = 0x0DB080)]
    public byte Property_OAMHarm { get; set; }
    public int[]? OamHarmAddresses { get; init; } = null;

    /// <summary>
    /// Gets or sets whether or not the sprite is completely harmless to the player.
    /// </summary>
    [PackedBitfieldAccessor(nameof(Property_OAMHarm))]
    public bool IsHarmless
    {
        get => Property_OAMHarm.BitIsSet(7);
        set => Property_OAMHarm = Property_OAMHarm.SetBit(7, value);
    }

    /// <summary>
    /// Gets or sets whether a sprite dies off screen during tile checks.
    /// </summary>
    [PackedBitfieldAccessor(nameof(Property_OAMHarm))]
    public bool KillWhenOffscreenDuringTileChecks
    {
        get => Property_OAMHarm.BitIsSet(6);
        set => Property_OAMHarm = Property_OAMHarm.SetBit(6, value);
    }

    /// <summary>
    /// When set, this sprite will use a special hardcoded tile collision check.
    /// </summary>
    [PackedBitfieldAccessor(nameof(Property_OAMHarm))]
    public bool UseTileHitbox68
    {
        get => Property_OAMHarm.BitIsSet(5);
        set => Property_OAMHarm = Property_OAMHarm.SetBit(5, value);
    }

    /// <summary>
    /// Gets or sets the number of objects this sprite uses.
    /// </summary>
    [PackedBitfieldAccessor(nameof(Property_OAMHarm))]
    public byte OamAllocation
    {
        get => Property_OAMHarm.GetField(0, 5);
        set => Property_OAMHarm = Property_OAMHarm.SetField(0, 5, value);
    }




    [PackedBitfield(nameof(IgnoreCollisionWhenRecoiling))]
    [PackedBitfield(nameof(BeeTarget))]
    [PackedBitfield(nameof(ImmuneToPowder))]
    [PackedBitfield(nameof(SurvivesBossPrep))]
    [PackedBitfield(nameof(BumpDamageClass))]
    [DisassemblyName(FileName = "bank_0D.asm", SymbolName = "SpriteData_Bump", Address = 0x0DB266)]
    public byte Property_BUMP { get; set; } = 0;
    public int[]? BumpAddresses { get; init; } = null;

    [PackedBitfieldAccessor(nameof(Property_BUMP))]
    public bool IgnoreCollisionWhenRecoiling
    {
        get => Property_BUMP.BitIsSet(7);
        set => Property_BUMP = Property_BUMP.SetBit(7, value);
    }

    [PackedBitfieldAccessor(nameof(Property_BUMP))]
    public bool BeeTarget
    {
        get => Property_BUMP.BitIsSet(6);
        set => Property_BUMP = Property_BUMP.SetBit(6, value);
    }

    [PackedBitfieldAccessor(nameof(Property_BUMP))]
    [ValueRange(0, 9)]
    public bool ImmuneToPowder
    {
        get => Property_BUMP.BitIsSet(5);
        set => Property_BUMP = Property_BUMP.SetBit(5, value);
    }

    [PackedBitfieldAccessor(nameof(Property_BUMP))]
    public bool SurvivesBossPrep
    {
        get => Property_BUMP.BitIsSet(4);
        set => Property_BUMP = Property_BUMP.SetBit(4, value);
    }

    [PackedBitfieldAccessor(nameof(Property_BUMP))]
    public byte BumpDamageClass
    {
        get => Property_BUMP.GetField(0, 4);
        set => Property_BUMP = Property_BUMP.SetField(0, 4, value);
    }


    [PackedBitfield(nameof(HasCustomDeathAnimation))]
    [PackedBitfield(nameof(IsInvulnerable))]
    [PackedBitfield(nameof(ShadowIsSmall))]
    [PackedBitfield(nameof(HasAShadow))]
    [PackedBitfield(nameof(InitialPalette))]
    [PackedBitfield(nameof(InitialUsesSecondNameTable))]
    [DisassemblyName(FileName = "bank_0D.asm", SymbolName = "SpriteData_OAMProp", Address = 0x0DB359)]
    public byte Property_OAMProp { get; set; }
    public int[]? OamPropAddresses { get; init; } = null;

    [PackedBitfieldAccessor(nameof(Property_OAMProp))]
    public bool HasCustomDeathAnimation
    {
        get => Property_OAMProp.BitIsSet(7);
        set => Property_OAMProp = Property_OAMProp.SetBit(7, value);
    }

    [PackedBitfieldAccessor(nameof(Property_OAMProp))]
    public bool IsInvulnerable
    {
        get => Property_OAMProp.BitIsSet(6);
        set => Property_OAMProp = Property_OAMProp.SetBit(6, value);
    }

    [PackedBitfieldAccessor(nameof(Property_OAMProp))]
    public bool ShadowIsSmall
    {
        get => Property_OAMProp.BitIsSet(5);
        set => Property_OAMProp = Property_OAMProp.SetBit(5, value);
    }

    [PackedBitfieldAccessor(nameof(Property_OAMProp))]
    public bool HasAShadow
    {
        get => Property_OAMProp.BitIsSet(4);
        set => Property_OAMProp = Property_OAMProp.SetBit(4, value);
    }

    [PackedBitfieldAccessor(nameof(Property_OAMProp))]
    [ValueRange(0, 7)]
    public byte InitialPalette
    {
        get => Property_OAMProp.GetField(1, 3);
        set => Property_OAMProp = Property_OAMProp.SetField(1, 3, value);
    }

    [PackedBitfieldAccessor(nameof(Property_OAMProp))]
    public bool InitialUsesSecondNameTable
    {
        get => Property_OAMProp.BitIsSet(0);
        set => Property_OAMProp = Property_OAMProp.SetBit(0, value);
    }


    [PackedBitfield(nameof(Palette))]
    [PackedBitfield(nameof(UsesSecondNameTable))]
    public byte DrawProperties { get; set; }

    [PackedBitfieldAccessor(nameof(DrawProperties))]
    [ValueRange(0, 7)]
    public byte Palette
    {
        get => DrawProperties.GetField(1, 3);
        set => DrawProperties = DrawProperties.SetField(1, 3, value);
    }

    [PackedBitfieldAccessor(nameof(DrawProperties))]
    public bool UsesSecondNameTable
    {
        get => DrawProperties.BitIsSet(0);
        set => DrawProperties = Property_OAMProp.SetBit(0, value);
    }
    public int[]? MiscPaletteAddresses { get; init; } = null;



    [PackedBitfield(nameof(UsesSingleLayerCollision))]
    [PackedBitfield(nameof(IgnoredByKillRooms))]
    [PackedBitfield(nameof(StaysActiveOffscreen))]
    [PackedBitfield(nameof(Hitbox))]
    [DisassemblyName(FileName = "bank_0D.asm", SymbolName = "SpriteData_Hitbox", Address = 0x0DB4CC)]
    public byte Property_HITBOX { get; set; }
    public int[]? HitboxAddresses { get; init; } = null;


    [PackedBitfieldAccessor(nameof(Property_HITBOX))]
    public bool UsesSingleLayerCollision
    {
        get => Property_HITBOX.BitIsSet(7);
        set => Property_HITBOX = Property_HITBOX.SetBit(7, value);
    }


    [PackedBitfieldAccessor(nameof(Property_HITBOX))]
    public bool IgnoredByKillRooms
    {
        get => Property_HITBOX.BitIsSet(6);
        set => Property_HITBOX = Property_HITBOX.SetBit(6, value);
    }


    [PackedBitfieldAccessor(nameof(Property_HITBOX))]
    public bool StaysActiveOffscreen
    {
        get => Property_HITBOX.BitIsSet(5);
        set => Property_HITBOX = Property_HITBOX.SetBit(5, value);
    }

    [PackedBitfieldAccessor(nameof(Property_OAMProp))]
    [ValueRange(0x00, 0x1F)]
    public byte Hitbox
    {
        get => Property_OAMProp.GetField(0, 5);
        set => Property_OAMProp = Property_OAMProp.SetField(0, 5, value);
    }


    [PackedBitfield(nameof(TileHitboxOffsets))]
    [PackedBitfield(nameof(DeflectsArrows))]
    [PackedBitfield(nameof(RefreshingHits))]
    [PackedBitfield(nameof(DiesLikeABoss))]
    [PackedBitfield(nameof(InvertPitBehavior))]
    [DisassemblyName(FileName = "bank_0D.asm", SymbolName = "SpriteData_TileInteraction", Address = 0x0DB53F)]
    public byte Property_TILEDIE { get; set; }
    public int[]? TileDieAddresses { get; init; } = null;

    [PackedBitfieldAccessor(nameof(Property_TILEDIE))]
    public byte TileHitboxOffsets
    {
        get => Property_TILEDIE.GetField(4, 4);
        set => Property_TILEDIE = Property_OAMProp.SetField(4, 4, value);
    }

    [PackedBitfieldAccessor(nameof(Property_TILEDIE))]
    public bool DeflectsArrows
    {
        get => Property_TILEDIE.BitIsSet(3);
        set => Property_TILEDIE = Property_TILEDIE.SetBit(3, value);
    }

    [PackedBitfieldAccessor(nameof(Property_TILEDIE))]
    public bool RefreshingHits
    {
        get => Property_TILEDIE.BitIsSet(2);
        set => Property_TILEDIE = Property_TILEDIE.SetBit(2, value);
    }

    [PackedBitfieldAccessor(nameof(Property_TILEDIE))]
    public bool DiesLikeABoss
    {
        get => Property_TILEDIE.BitIsSet(1);
        set => Property_TILEDIE = Property_TILEDIE.SetBit(1, value);
    }

    [PackedBitfieldAccessor(nameof(Property_TILEDIE))]
    public bool InvertPitBehavior
    {
        get => Property_TILEDIE.BitIsSet(0);
        set => Property_TILEDIE = Property_TILEDIE.SetBit(0, value);
    }

    [PackedBitfield(nameof(LimitedPitAndConveryorInteractions))]
    [PackedBitfield(nameof(UniqueWaterCheck))]
    [PackedBitfield(nameof(BlockedByShield))]
    [PackedBitfield(nameof(UsesAlternateDamageSound))]
    [PackedBitfield(nameof(PrizePack))]
    [DisassemblyName(FileName = "bank_0D.asm", SymbolName = "SpriteData_PrizePack", Address = 0x0DB632)]
    public byte Property_PRIZE { get; set; }
    public int[]? PrizeAddresses { get; init; } = null;


    [PackedBitfieldAccessor(nameof(Property_PRIZE))]
    public bool LimitedPitAndConveryorInteractions
    {
        get => Property_PRIZE.BitIsSet(7);
        set => Property_PRIZE = Property_PRIZE.SetBit(7, value);
    }

    [PackedBitfieldAccessor(nameof(Property_PRIZE))]
    public bool UniqueWaterCheck
    {
        get => Property_PRIZE.BitIsSet(6);
        set => Property_PRIZE = Property_PRIZE.SetBit(6, value);
    }

    [PackedBitfieldAccessor(nameof(Property_PRIZE))]
    public bool BlockedByShield
    {
        get => Property_PRIZE.BitIsSet(5);
        set => Property_PRIZE = Property_PRIZE.SetBit(5, value);
    }

    [PackedBitfieldAccessor(nameof(Property_PRIZE))]
    public bool UsesAlternateDamageSound
    {
        get => Property_PRIZE.BitIsSet(4);
        set => Property_PRIZE = Property_PRIZE.SetBit(4, value);
    }

    [PackedBitfieldAccessor(nameof(Property_PRIZE))]
    [ValueRange(0, 7)]
    public byte PrizePack
    {
        get => Property_PRIZE.GetField(0, 4);
        set => Property_PRIZE = Property_PRIZE.SetField(0, 4, value);
    }




    [PackedBitfield(nameof(PersistsOffscreenOnOverworld))]
    [PackedBitfield(nameof(AlwaysDiesOffscreen))]
    [PackedBitfield(nameof(UnusedStatueMarker))]
    [PackedBitfield(nameof(AncillaeCheckDirectionAgainst))]
    [PackedBitfield(nameof(UsesProjectileCollision))]
    [PackedBitfield(nameof(ImmuneToSwordAndHammer))]
    [PackedBitfield(nameof(BonkableItemMarker))]
    [PackedBitfield(nameof(DoesNotPermanentlyDieInUnderworld))]
    [DisassemblyName(FileName = "bank_0D.asm", SymbolName = "SpriteData_Deflection", Address = 0x0DB725)]
    public byte Property_DEFLECT { get; set; }
    public int[]? DeflectAddresses { get; init; } = null;


    [PackedBitfieldAccessor(nameof(Property_DEFLECT))]
    public bool PersistsOffscreenOnOverworld
    {
        get => Property_DEFLECT.BitIsSet(7);
        set => Property_DEFLECT = Property_DEFLECT.SetBit(7, value);
    }


    [PackedBitfieldAccessor(nameof(Property_DEFLECT))]
    public bool AlwaysDiesOffscreen
    {
        get => Property_DEFLECT.BitIsSet(6);
        set => Property_DEFLECT = Property_DEFLECT.SetBit(6, value);
    }


    [PackedBitfieldAccessor(nameof(Property_DEFLECT))]
    public bool UnusedStatueMarker
    {
        get => Property_DEFLECT.BitIsSet(5);
        set => Property_DEFLECT = Property_DEFLECT.SetBit(5, value);
    }


    [PackedBitfieldAccessor(nameof(Property_DEFLECT))]
    public bool AncillaeCheckDirectionAgainst
    {
        get => Property_DEFLECT.BitIsSet(4);
        set => Property_DEFLECT = Property_DEFLECT.SetBit(4, value);
    }


    [PackedBitfieldAccessor(nameof(Property_DEFLECT))]
    public bool UsesProjectileCollision
    {
        get => Property_DEFLECT.BitIsSet(3);
        set => Property_DEFLECT = Property_DEFLECT.SetBit(3, value);
    }


    [PackedBitfieldAccessor(nameof(Property_DEFLECT))]
    public bool ImmuneToSwordAndHammer
    {
        get => Property_DEFLECT.BitIsSet(2);
        set => Property_DEFLECT = Property_DEFLECT.SetBit(2, value);
    }

    [PackedBitfieldAccessor(nameof(Property_DEFLECT))]
    public bool BonkableItemMarker
    {
        get => Property_DEFLECT.BitIsSet(1);
        set => Property_DEFLECT = Property_DEFLECT.SetBit(1, value);
    }

    [PackedBitfieldAccessor(nameof(Property_DEFLECT))]
    public bool DoesNotPermanentlyDieInUnderworld
    {
        get => Property_DEFLECT.BitIsSet(0);
        set => Property_DEFLECT = Property_DEFLECT.SetBit(0, value);
    }

    // This can't actually be changed easily, but it's useful to know
    public bool ImmuneToAncillae { get; init; }



    [ValueRange(0, 5)]
    public byte DamageClass0Subclass { get; set; }

    [ValueRange(0, 3)]
    public byte DamageClass1Subclass { get; set; }

    [ValueRange(0, 4)]
    public byte DamageClass2Subclass { get; set; }

    [ValueRange(0, 3)]
    public byte DamageClass3Subclass { get; set; }

    [ValueRange(0, 3)]
    public byte DamageClass4Subclass { get; set; }

    [ValueRange(0, 3)]
    public byte DamageClass5Subclass { get; set; }

    [ValueRange(0, 3)]
    public byte DamageClass6Subclass { get; set; }

    [ValueRange(0, 5)]
    public byte DamageClass7Subclass { get; set; }

    [ValueRange(0, 6)]
    public byte DamageClass8Subclass { get; set; }

    [ValueRange(0, 3)]
    public byte DamageClass9Subclass { get; set; }

    [ValueRange(0, 4)]
    public byte DamageClassASubclass { get; set; }

    [ValueRange(0, 5)]
    public byte DamageClassBSubclass { get; set; }

    [ValueRange(0, 4)]
    public byte DamageClassCSubclass { get; set; }

    [ValueRange(0, 3)]
    public byte DamageClassDSubclass { get; set; }

    [ValueRange(0, 3)]
    public byte DamageClassESubclass { get; set; }

    [Values(0, 1, 2, 3, 7)] // yeah... this one is weird and not contiguous
    public byte DamageClassFSubclass { get; set; }

    internal static SpriteProperties FromYaml(string name, YamlSpriteData serial)
    {
        Span<byte> damageFallback = stackalloc byte[16];
        damageFallback.Clear();

        Span<byte> damageSubclasses = serial.DamageSubclasses ?? damageFallback;

        return new()
        {
            Name = name,
            ID = serial.ID,
            OwnsId = serial.OwnsId,
            AlwaysDiesOffscreen = serial.AlwaysDiesOffscreen,
            InvertPitBehavior = serial.InvertPitBehavior,
            KillWhenOffscreenDuringTileChecks = serial.KillWhenOffscreenDuringTileChecks,
            PersistsOffscreenOnOverworld = serial.PersistsOffscreenOnOverworld,
            StaysActiveOffscreen = serial.StaysActiveOffscreen,
            SurvivesBossPrep = serial.SurvivesBossPrep,
            UsesAlternateDamageSound = serial.UsesAlternateDamageSound,
            BeeTarget = serial.BeeTarget,
            BlockedByShield = serial.BlockedByShield,
            DiesLikeABoss = serial.DiesLikeABoss,
            BumpAddresses = serial.BumpAddresses,
            BumpDamageClass = serial.BumpDamageClass,
            AncillaeCheckDirectionAgainst = serial.AncillaeCheckDirectionAgainst,
            HasCustomDeathAnimation = serial.HasCustomDeathAnimation,
            InitialUsesSecondNameTable = serial.InitialUsesSecondNameTable,
            InitialPalette = serial.InitialPalette,
            DeflectAddresses = serial.DeflectAddresses,
            DeflectsArrows = serial.DeflectsArrows,
            IsHarmless = serial.IsHarmless,
            HasAShadow = serial.HasAShadow,
            Property_HP = serial.Health,
            Hitbox = serial.Hitbox,
            HitboxAddresses = serial.HitboxAddresses,
            HpAddresses = serial.HpAddresses,
            IgnoreCollisionWhenRecoiling = serial.IgnoreCollisionWhenRecoiling,
            ImmuneToAncillae = serial.ImmuneToAncillae,
            ImmuneToSwordAndHammer = serial.ImmuneToSwordAndHammer,
            IsInvulnerable = serial.IsInvulnerable,
            BonkableItemMarker = serial.BonkableItemMarker,
            UsesProjectileCollision = serial.UsesProjectileCollision,
            UnusedStatueMarker = serial.UnusedStatueMarker,
            IgnoredByKillRooms = serial.IgnoredByKillRooms,
            LimitedPitAndConveryorInteractions = serial.LimitedPitAndConveryorInteractions,
            DoesNotPermanentlyDieInUnderworld = serial.DoesNotPermanentlyDieInUnderworld,
            OamAllocation = serial.OamAllocation,
            UsesSecondNameTable = serial.UsesSecondNameTable,
            OamHarmAddresses = serial.OamHarmAddresses,
            OamPropAddresses = serial.OamPropAddresses,
            Palette = serial.Palette,
            MiscPaletteAddresses = serial.MiscPaletteAddresses,
            ImmuneToPowder = serial.ImmuneToPowder,
            PrizeAddresses = serial.PrizeAddresses,
            PrizePack = serial.PrizePack,
            RefreshingHits = serial.RefreshingHits,
            UsesSingleLayerCollision = serial.UsesSingleLayerCollision,
            ShadowIsSmall = serial.ShadowIsSmall,
            UseTileHitbox68 = serial.UseTileHitbox68,
            UniqueWaterCheck = serial.UniqueWaterCheck,
            TileHitboxOffsets = serial.TileHitboxOffsets,
            TileDieAddresses = serial.TileDieAddresses,
            DamageClass0Subclass = damageSubclasses[0x0],
            DamageClass1Subclass = damageSubclasses[0x1],
            DamageClass2Subclass = damageSubclasses[0x2],
            DamageClass3Subclass = damageSubclasses[0x3],
            DamageClass4Subclass = damageSubclasses[0x4],
            DamageClass5Subclass = damageSubclasses[0x5],
            DamageClass6Subclass = damageSubclasses[0x6],
            DamageClass7Subclass = damageSubclasses[0x7],
            DamageClass8Subclass = damageSubclasses[0x8],
            DamageClass9Subclass = damageSubclasses[0x9],
            DamageClassASubclass = damageSubclasses[0xA],
            DamageClassBSubclass = damageSubclasses[0xB],
            DamageClassCSubclass = damageSubclasses[0xC],
            DamageClassDSubclass = damageSubclasses[0xD],
            DamageClassESubclass = damageSubclasses[0xE],
            DamageClassFSubclass = damageSubclasses[0xF],
        };
    }
}
