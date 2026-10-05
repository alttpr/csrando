namespace Randomizer.Games.Alttp;

public record class Sprite(string Name, byte Id) {
    private static readonly Lazy<Dictionary<string, Sprite>> _sprites = new(() => LoadSprites().ToDictionary(k => k.Name));

    public string DefeatName { get; init; } = Name;
    public byte?[] Sheets { get; init; } = [null, null, null, null];
    public YamlSpriteFlags Flags { get; init; }
    public string[]? NotWith { get; init; }
    public byte SubType { get; init; }
    public string? FallingSpriteFor { get; init; }
    public int Priority { get; init; }
    public int Weight { get; init; }

    /// <summary>
    /// When <see langword="false"/>, this indicates a sprite is a variant that should
    /// not provide any randomization for its ID other than the delegates it provides.
    /// </summary>
    public bool OwnsId { get; init; } = true;

    // QUESTION should these be moved out of this type and moved to answer?
    #region ROM PROPERTIES

    public byte Property_HP { get; set; }

    public int[]? HpAddresses { get; init; } = null;

    public byte Property_BUMP { get; set; } = 0;
    public int[]? BumpAddresses { get; init; } = null;


    public bool IgnoreCollisionWhenRecoiling {
        get => Property_BUMP.BitIsSet(7);
        set => Property_BUMP = Property_BUMP.SetBit(7, value);
    }

    public bool BeeTarget {
        get => Property_BUMP.BitIsSet(6);
        set => Property_BUMP = Property_BUMP.SetBit(6, value);
    }

    public bool ImmuneToPoweder {
        get => Property_BUMP.BitIsSet(5);
        set => Property_BUMP = Property_BUMP.SetBit(5, value);
    }

    public byte BumpDamageClass {
        get => Property_BUMP.GetField(0, 4);
        set => Property_BUMP = Property_BUMP.SetField(value, 0, 4);
    }

    #endregion


    public static Sprite Get(string name)
        => _sprites.Value.GetValueOrDefault(name)
        ?? throw new ArgumentException($"No such sprite: {name}", nameof(name));

    public static IEnumerable<Sprite> All() => _sprites.Value.Values;

    private static IEnumerable<Sprite> LoadSprites() {
        var spriteData = YamlReader.LoadSprites();
        foreach (var (name, sprite) in spriteData) {
            yield return new(name, sprite.Id) {
                Sheets = sprite.Sheets,
                Flags = sprite.Flags,
                SubType = sprite.SubType,
                DefeatName = sprite.AlternativeName ?? name,
                FallingSpriteFor = sprite.FallingSpriteFor,
                Priority = sprite.Priority,
                NotWith = sprite.NotWith,
                Weight = sprite.Weight,
            };
        }
    }

}
