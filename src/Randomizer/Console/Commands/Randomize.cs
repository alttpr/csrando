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
    //private readonly Argument<FileInfo> _input_file = new("input_file", "base ROM to randomize");
    //private readonly Argument<DirectoryInfo> _output_directory = new("output_directory", "where to place randomized ROM");
    //private readonly Option<bool> _unrandomized = new("unrandomized", "do not apply randomization to the ROM");
    //private readonly Option<bool> _spoiler = new("spoiler", "generate a spoiler file");
    //private readonly Option<HeartBeepSpeed> _heartbeep = new("heartbeep", "set heart beep speed");
    //private readonly Option<HeartColor> _heartcolor = new("heartcolor", "set heart color");
    //private readonly Option<MenuSpeed> _menuspeed = new("menu-speed", "menu speed");
    private readonly Option<GameGoal> _goal = new("goal", () => GameGoal.Ganon, "set game goal");
    private readonly Option<GameState> _state = new("state", () => GameState.Standard, "set game state");
    private readonly Option<WeaponMode> _weapons = new("weapons", "set weapons mode");
    private readonly Option<GlitchedMode> _glitches = new("glitches", "set glitches");
    private readonly Option<ItemPlacement> _item_placement = new("item_placement", "set item placement rules");
    private readonly Option<DungeonItems> _dungeon_items = new("dungeon_items", "set dungeon item placement");
    private readonly Option<AccessbilityMode> _accessibility = new("accessibility", "set item/location accessibility");
    private readonly Option<ItemPool> _item_pool = new("item_pool", "set item pool");
    private readonly Option<ItemFunctionality> _item_functionality = new("item_functionality", "set item functionality");
    private readonly Option<OnOff> _hints = new("hints", "set hints on or off");
    //private readonly Option<bool> _skipmd5 = new("skip-md5", "do not validate md5 of base ROM");
    private readonly Option<bool> _tournament = new("tournament", "enable tournament mode");
    //private readonly Option<bool> _nomusic = new("no-music", "mute all music");
    //private readonly Option<bool> _norom = new("no-rom", "do not generate output ROM");
    private readonly Option<int> _bulk = new("bulk", "generate multiple ROMs");
    private static readonly string[] _crystalAmount = { "random", "0", "1", "2", "3", "4", "5", "6", "7" };
    private readonly Option<string> _crystals_ganon = new Option<string>("crystals_ganon", () => "7", "set ganon crystal requirement").FromAmong(_crystalAmount);
    private readonly Option<string> _crystals_tower = new Option<string>("crystals_tower", () => "7", "set ganon tower crystal requirement").FromAmong(_crystalAmount);
    //private readonly Option<FileInfo> _sprite = new("sprite", "sprite file to change links graphics [zspr format]");
    //private readonly Option<bool> _quickswap = new("quickswap", "set quickswap");


    public Randomize()
        : base("randomize", "Generate a randomized ROM.")
    {
        //Add(_input_file);
        //Add(_output_directory);
        //Add(_unrandomized);
        //Add(_spoiler);
        //Add(_heartbeep);
        //Add(_heartcolor);
        //Add(_menuspeed);
        Add(_goal);
        Add(_state);
        Add(_weapons);
        Add(_glitches);
        Add(_item_placement);
        Add(_dungeon_items);
        Add(_accessibility);
        Add(_item_pool);
        Add(_item_functionality);
        Add(_hints);
        //Add(_skipmd5);
        Add(_tournament);
        //Add(_nomusic);
        //Add(_norom);
        Add(_bulk);
        Add(_crystals_ganon);
        Add(_crystals_tower);
        //Add(_sprite);
        //Add(_quickswap);
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
            var logic = context.ParseResult.GetValueForOption(_glitches);

            var randomizer = new Randomizer(new[] { new Dictionary<string, object>()
            {
                { "mode.state", context.ParseResult.GetValueForOption(_state).ToString().ToLowerInvariant() },
                { "itemPlacement", context.ParseResult.GetValueForOption(_item_placement).ToString().ToLowerInvariant() },
                { "dungeonItems", context.ParseResult.GetValueForOption(_dungeon_items).ToString().ToLowerInvariant() },
                { "accessibility", context.ParseResult.GetValueForOption(_accessibility).ToString().ToLowerInvariant() },
                { "goal", context.ParseResult.GetValueForOption(_goal).ToString().ToLowerInvariant() },
                { "crystals.ganon", crystals_ganon },
                { "crystals.tower", crystals_tower },
                { "entrances", "none" },
                { "mode.weapons", context.ParseResult.GetValueForOption(_weapons).ToString().ToLowerInvariant() },
                { "tournament", context.ParseResult.GetValueForOption(_tournament) },
                { "spoil.Hints", context.ParseResult.GetValueForOption(_hints).ToString().ToLowerInvariant() },
                { "logic", logic.ToString() },
                { "item.pool", context.ParseResult.GetValueForOption(_item_pool).ToString().ToLowerInvariant() },
                { "item.functionality", context.ParseResult.GetValueForOption(_item_functionality).ToString().ToLowerInvariant() },
                { "enemizer.bossShuffle", "random" },
                { "enemizer.enemyShuffle", "none" },
                { "enemizer.enemyDamage", "default" },
                { "enemizer.enemyHealth", "default" },
                { "enemizer.potShuffle", "off" },
                //"region.shopSupply" => "shuffled",
                //"equipment" => [
                //    // for testing
                //    "BossHeartContainer:0",
                //    "BossHeartContainer:0",
                //    "BossHeartContainer:0",
                //    "BossHeartContainer:0",
                //    "BossHeartContainer:0",
                //    "BossHeartContainer:0",
                //    "BossHeartContainer:0",
                //    "BossHeartContainer:0",
                //    "BossHeartContainer:0",
                //    "BossHeartContainer:0",
                //    "ThreeHundredRupees:0",
                //    "ThreeHundredRupees:0",
                //    "PegasusBoots:0",
                //    "BowAndArrows:0",
                //    "ProgressiveSword:0",
                //    "ProgressiveSword:0",
                //    "ProgressiveSword:0",
                //    "ProgressiveGlove:0",
                //    "ProgressiveGlove:0",
                //    "Lamp:0",
                //    "OcarinaActive:0",
                //    "CaneOfSomaria:0",
                //    "FireRod:0",
                //    "MagicMirror:0",
                //    "Flippers:0",
                //    "MoonPearl:0",
                //    "Hookshot:0",
                //    "Hammer:0",
                //],
            } });
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

//public enum HeartBeepSpeed { Off, Half, Quarter, Double, Normal }
//public enum HeartColor { Random, Blue, Green, Yellow, Red }
//public enum MenuSpeed { }
public enum GameGoal { Ganon, FastGanon, Dungeons, Pedestal }
public enum GameState { Standard, Inverted, Open }
public enum WeaponMode { Randomized, Assured, Vanilla, Swordless }
public enum GlitchedMode
{
    NoGlitches,
    None = NoGlitches,
    OverworldGlitches,
    Overworld_Glitches = OverworldGlitches,
    MajorGlitches,
    Major_Glitches = MajorGlitches,
    NoLogic,
    No_Logic = NoLogic,
}
public enum ItemPlacement { Basic, Advdanced }
public enum DungeonItems { Standard }
public enum AccessbilityMode { Items, Locations, None }
public enum ItemPool { Normal, Hard, Expert }
public enum ItemFunctionality { Normal, Hard, Expert }
public enum OnOff { On, Off }
