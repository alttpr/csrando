namespace Randomizer.Games.Alttp;

public record class Sprite(string Name, byte Id)
{
    private static readonly Lazy<Dictionary<string, Sprite>> _sprites = new(() => LoadSprites().ToDictionary(k => k.Name));

    public string DefeatName { get; init; } = Name;
    public byte?[] Sheets { get; init; } = [null, null, null, null];
    public YamlSpriteFlags Flags { get; init; }
    public string[]? NotWith { get; init; }
    public byte SubType { get; init; }
    public string? FallingSpriteFor { get; init; }
    public int Priority { get; init; }
    public int Weight { get; init; }

    public SpriteProperties? Properties { get; init; } = null;

    public static Sprite Get(string name)
        => _sprites.Value.GetValueOrDefault(name)
        ?? throw new ArgumentException($"No such sprite: {name}", nameof(name));

    public static IEnumerable<Sprite> All() => _sprites.Value.Values;

    private static IEnumerable<Sprite> LoadSprites()
    {

        // TODO need to load sprites properties first and then look up their data
        var spriteData = YamlReader.LoadSprites();

        foreach (var (name, sprite) in spriteData)
        {
            yield return new(name, sprite.Id)
            {
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
