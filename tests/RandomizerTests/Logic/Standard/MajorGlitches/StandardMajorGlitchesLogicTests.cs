namespace RandomizerTests.Logic.Standard.MajorGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;

public abstract class StandardMajorGlitchesLogicTests : LogicTestBase
{
    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void ClassInit(TestContext testContext)
    {
        Assert.Inconclusive("Skipped until logic is implemented");
    }
    protected override WorldConfig GetWorldConfig() => new()
    {
        Alttp = new()
        {
            Glitches = GlitchesOption.Major,
            State = StateOption.Standard,
        }
    };
}
