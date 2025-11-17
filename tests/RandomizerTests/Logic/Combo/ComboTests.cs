namespace RandomizerTests.Logic.Combo;


using Randomizer.Games;
using Randomizer.Games.Combo;

public class ComboTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Combo = new()
        {
            InitialGame = "alttp",
            Race = false
        },
        Alttp = new()
        {
            State = Randomizer.Games.Alttp.StateOption.Open,
            Glitches = Randomizer.Games.Alttp.GlitchesOption.None,
        },
        SuperMetroid = new()
        {
            Logic = Randomizer.Games.SuperMetroid.Logic.Basic
        },
        Metroid = new()
        {
        },
        Zelda1 = new()
        {
        },
    };
}
