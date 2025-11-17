namespace RandomizerTests.Logic.SuperMetroid.Basic;

using Randomizer.Games;
using Randomizer.Games.SuperMetroid;

public abstract class BasicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        SuperMetroid = new()
        {
            Logic = Logic.Basic,
        }   
    };
}
