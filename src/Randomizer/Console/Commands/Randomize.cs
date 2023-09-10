namespace AlttpRandomizer.Console.Commands;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using AlttpRandomizer.Graph;

/**
 * Run randomizer as command.
 */
internal sealed class Randomize : Command
{
    private readonly Option<GoalOption> _goal = new("goal", () => GoalOption.Ganon, "set game goal");
    private readonly Option<StateOption> _state = new("state", () => StateOption.Standard, "set game state");
    private readonly Option<WeaponOption> _weapons = new("weapons", () => WeaponOption.Randomized, "set weapons mode");
    private readonly Option<GlitchesOption> _glitches = new("glitches", () => GlitchesOption.None, "set glitches");
    private readonly Option<AccessibilityOption> _accessibility = new("accessibility", "set item/location accessibility");
    private readonly Option<BossShuffleOption> _boss_shuffle = new("bossshuffle", () => BossShuffleOption.None, "set boss shuffle mode");
    private readonly Option<EntranceShuffleOption> _entrance_shuffle = new("entrance", () => EntranceShuffleOption.None, "set entrance shuffle mode");
    private readonly Option<ShopSupplyOption> _shop_supply = new("shopsupply", () => ShopSupplyOption.Normal, "set shop supply shuffle mode");
    private static readonly string[] _crystalAmount = { "random", "0", "1", "2", "3", "4", "5", "6", "7" };
    private readonly Option<string> _crystals_ganon = new Option<string>("crystals_ganon", () => "7", "set ganon crystal requirement").FromAmong(_crystalAmount);
    private readonly Option<string> _crystals_tower = new Option<string>("crystals_tower", () => "7", "set ganon tower crystal requirement").FromAmong(_crystalAmount);
    private readonly Option<List<TechOption>> _tech = new Option<List<TechOption>>("tech", "set allowed techs").FromAmong(Enum.GetNames(typeof(TechOption)));
    private readonly Option<int> _bulk = new("bulk", "generate multiple ROMs");
    public Randomize()
        : base("randomize", "Generate a randomized ROM.")
    {
        Add(_goal);
        Add(_state);
        Add(_weapons);
        Add(_glitches);
        Add(_accessibility);
        Add(_boss_shuffle);
        Add(_entrance_shuffle);
        Add(_shop_supply);
        Add(_crystals_ganon);
        Add(_crystals_tower);
        Add(_tech);
        Add(_bulk);

        this.SetHandler(context => context.ExitCode = Handle(context));
    }

    /**
     * Execute the console command.
     *
     * @return mixed
     */
    public int Handle(InvocationContext context)
    {
        int bulk = Math.Max(context.ParseResult.GetValueForOption(_bulk), 1);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < bulk; i++)
        {
            string? crystals_ganonS = context.ParseResult.GetValueForOption(_crystals_ganon);
            if (crystals_ganonS == "random" || !int.TryParse(crystals_ganonS, out int crystals_ganon))
                crystals_ganon = PHP.get_random_int(0, 7);
            string? crystals_towerS = context.ParseResult.GetValueForOption(_crystals_tower);
            if (crystals_towerS == "random" || !int.TryParse(crystals_towerS, out int crystals_tower))
                crystals_tower = PHP.get_random_int(0, 7);

            var randomizer = new Randomizer(
            new[]
            {
                new RandomizerConfig
                {
                    Accessibility = context.ParseResult.GetValueForOption(_accessibility),
                    Goal = context.ParseResult.GetValueForOption(_goal),
                    State = context.ParseResult.GetValueForOption(_state),
                    Glitches = context.ParseResult.GetValueForOption(_glitches),
                    EntranceShuffle = context.ParseResult.GetValueForOption(_entrance_shuffle),
                    BossShuffle = context.ParseResult.GetValueForOption(_boss_shuffle),
                    RegionShopSupply = context.ParseResult.GetValueForOption(_shop_supply),
                    CrystalsGanon = crystals_ganon,
                    CrystalsTower = crystals_tower,
                    Weapon = context.ParseResult.GetValueForOption(_weapons),
                    Techs = context.ParseResult.GetValueForOption(_tech) ?? new(),
                },
            });
            var worlds = randomizer.Randomize();
            if (!randomizer.CollectItems().Has("Triforce:0"))
            {
                throw new Exception("Game Unwinnable");
            }

            // this contains the randomized worlds that would go into a ROM/BPS
            _ = worlds;
        }
        info("Randomization took {0}", sw.Elapsed);

        return 0;
    }

    private void info(string format, params object[] args)
    {
        System.Console.WriteLine(format, args);
    }
    private void error(string format, params object[] args)
    {
        var previousColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine(format, args);
        System.Console.ForegroundColor = previousColor;
    }
}
