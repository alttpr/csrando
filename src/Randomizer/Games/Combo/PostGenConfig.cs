namespace Randomizer.Games.Combo;

using Randomizer.Games.Metadata;

/// <summary>
/// Post-generation settings that apply to the combo ROM as a whole rather than to a single
/// game. They are keyed under the hidden "combo" game id and patched into the shared
/// configuration block at $FFFF00 (file offset 0x7FFF00).
/// </summary>
[PostGenSettingsFor("combo", Target = RandomizerTarget.Combo)]
public sealed class PostGenConfig
{
    [PostGenId("msu_volume")]
    [Name("MSU-1 Volume")]
    [Description("Change this to adjust the volume.")]
    [ValueRange(0, 255)]
    [NumberPatch(0x7FFF06)] // config_msu_volume, low byte
    public int MsuVolume { get; init; } = 0x7F;
}
