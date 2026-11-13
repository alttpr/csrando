namespace Randomizer.Games.Alttp;


public record class Sprite(string Name, byte Id)
{
    public string DefeatName {
        get;
        init
        {
            field = value;
            DefeatItem = $"Defeat{value}";
        }
    } = Name;

    public byte?[] Sheets { get; init; } = [null, null, null, null];
    public YamlSpriteFlags Flags { get; init; }
    public string[]? NotWith { get; init; }
    public byte SubType { get; init; }
    public string? FallingSpriteFor { get; init; }
    public int Priority { get; init; }
    public int Weight { get; init; }
    public bool IgnoredByKillRooms => Properties?.IgnoredByKillRooms ?? false;
    public byte Health => Properties?.Property_HP ?? 0;

    public SpriteProperties? Properties { get; init; }

    public string DefeatItem { get; private init; } = string.Empty;
}
