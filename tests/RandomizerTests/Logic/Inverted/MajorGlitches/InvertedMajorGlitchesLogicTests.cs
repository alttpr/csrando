using Randomizer.Games;
using Randomizer.Games.Alttp;

namespace RandomizerTests.Logic.Inverted.MajorGlitches;

public abstract class InvertedMajorGlitchesLogicTests : LogicTestBase
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
            State = StateOption.Inverted,
        }
    };
}
