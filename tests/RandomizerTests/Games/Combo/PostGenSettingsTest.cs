namespace RandomizerTests.Games.Combo;

using System.Linq;
using Randomizer.Games;
using Randomizer.Games.Metadata;

[TestClass]
public sealed class PostGenSettingsTest
{
    private const string ComboGameId = "combo";
    private const string MsuVolumeId = "msu_volume";

    // config_msu_volume in the shared configuration block ($FFFF06 -> file offset 0x7FFF06).
    private const int MsuVolumeAddress = 0x7FFF06;

    private static MetaPostGenSetting MsuVolumeSetting()
    {
        var settings = PostGenSettingsBuilder.Build(RandomizerTarget.Combo);
        Assert.IsTrue(settings.ContainsKey(ComboGameId), "Combo metadata should expose combo-wide post-gen settings.");

        var option = settings[ComboGameId].Options.SingleOrDefault(o => o.Id == MsuVolumeId);
        Assert.IsNotNull(option, "Combo post-gen settings should include the MSU-1 volume option.");
        return option!;
    }

    [TestMethod]
    public void MsuVolume_IsANumericSettingOverTheFullByteRange()
    {
        var option = MsuVolumeSetting();

        Assert.AreEqual("number", option.Type);
        Assert.AreEqual(0, option.Min);
        Assert.AreEqual(0xFF, option.Max);
        Assert.AreEqual(1, option.Step);
        Assert.AreEqual(0x7F, option.Default);
        Assert.IsFalse(string.IsNullOrWhiteSpace(option.Name));
        Assert.IsFalse(string.IsNullOrWhiteSpace(option.Description));
    }

    [TestMethod]
    public void MsuVolume_WritesASingleByteToTheSharedConfigBlock()
    {
        var option = MsuVolumeSetting();

        Assert.IsNotNull(option.Patches);
        var patch = option.Patches!.Single();
        Assert.AreEqual(MsuVolumeAddress, patch.TargetAddress);
        Assert.AreEqual(1, patch.Length);

        // Numeric settings carry the value itself, so they never declare toggle/select payloads.
        Assert.IsNull(option.On);
        Assert.IsNull(option.Off);
        Assert.IsNull(option.Choices);
    }

    [TestMethod]
    public void ComboWideSettings_AreNotEmittedForOtherRandomizers()
    {
        var settings = PostGenSettingsBuilder.Build(RandomizerTarget.Alttpr);
        Assert.IsFalse(
            settings.ContainsKey(ComboGameId),
            "Combo-wide post-gen settings only exist in the combined ROM.");
    }

    [TestMethod]
    public void ExistingToggleAndSelectSettingsStillBuild()
    {
        var settings = PostGenSettingsBuilder.Build(RandomizerTarget.Combo);
        Assert.IsTrue(settings.ContainsKey("alttp"));

        var alttp = settings["alttp"].Options;
        Assert.IsTrue(alttp.Any(o => o.Type == "toggle" && o.On is not null));
        Assert.IsTrue(alttp.Any(o => o.Type == "select" && o.Choices is { Count: > 0 }));
    }
}
