namespace RandomizerTests.Logic.Open.MajorGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;

public abstract class OpenMajorGlitchesLogicTests : LogicTestBase
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
            State = StateOption.Open,
        }
    };
}
