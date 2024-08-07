namespace RandomizerWeb.Server.Wrapper;

using Randomizer.Graph;
using Randomizer.Shared.Contracts;
using Randomizer.Shared.Models;
using System.ComponentModel;
using static Randomizer.Shared.Contracts.RandomizerOptionType;

public class RandomizerOptions
{
    public static List<IRandomizerOption> List { get; } = new List<IRandomizerOption> {
            CreateEnumOption<Goal>("Goal"),
            CreateEnumOption<KeyShuffle>("Key Shuffle"),
            CreateEnumOption<OpenTower>("Open Ganon's Tower"),
            CreateEnumOption<GanonVulnerable>("Ganon Vulnerable"),
            CreateEnumOption<OpenSMTourian>("Open SM Tourian"),
            CreateEnumOption<Z1Triforces>("Open Z1 Level Nine"),
            CreateEnumOption<Z1EntranceShuffle>("Z1 Entrance Shuffle"),
            CreateSeedOption(),
            CreateBoolOption("Race", "Race ROM (no spoilers)", false),
            //CreateEnumOption<GameMode>("Game mode"),
            //CreatePlayerOption(),
    };

    static RandomizerOption CreateEnumOption<TEnum>(string description, string defaultOption = null) where TEnum : Enum
    {
        var enumType = typeof(TEnum);
        return new RandomizerOption
        {
            Key = enumType.Name.ToLower(),
            Description = description,
            Type = Dropdown,
            Default = string.IsNullOrEmpty(defaultOption) ? GetDefaultValue<TEnum>().ToLowerString() : defaultOption,
            Values = Enum.GetValues(enumType).Cast<Enum>().ToDictionary(k => k.ToLowerString(), v => v.GetDescription()),
        };
    }

    static TEnum GetDefaultValue<TEnum>() where TEnum : Enum
    {
        var enumType = typeof(TEnum);
        var attrs = (DefaultValueAttribute[])enumType.GetCustomAttributes(typeof(DefaultValueAttribute), false);
        return (attrs?.Length ?? 0) > 0 ? (TEnum)attrs[0].Value : default;
    }

    static RandomizerOption CreateBoolOption(string name, string description, bool defaultOption = false)
    {
        return new RandomizerOption
        {
            Key = name.ToLower(),
            Description = description,
            Type = Checkbox,
            Default = defaultOption.ToString().ToLower(),
            Values = new Dictionary<string, string>(),
        };
    }

    static RandomizerOption CreateSeedOption() =>
        new RandomizerOption { Key = "seed", Description = "Seed", Type = RandomizerOptionType.Seed };

    static RandomizerOption CreatePlayerOption() =>
        new RandomizerOption { Key = "players", Description = "Players", Type = Players, Default = "2" };

    public static Config Parse(IDictionary<string, string> options)
    {
        return new Config
        {
            GameMode = ParseOption(options, GameMode.Normal),
            Goal = ParseOption(options, Goal.DefeatAll),
            OpenTower = ParseOption(options, OpenTower.SevenCrystals),
            GanonVulnerable = ParseOption(options, GanonVulnerable.SevenCrystals),
            OpenSMTourian = ParseOption(options, OpenSMTourian.FourBosses),
            Z1Triforces = ParseOption(options, Z1Triforces.EightTriforces),
            Z1EntranceShuffle = ParseOption(options, Z1EntranceShuffle.None),
            KeyShuffle = ParseOption(options, KeyShuffle.None),
            Race = ParseOption(options, "Race", false),
        };
    }

    static TEnum ParseOption<TEnum>(IDictionary<string, string> options, TEnum defaultValue) where TEnum : Enum
    {
        var enumKey = typeof(TEnum).Name.ToLower();
        if (options.ContainsKey(enumKey))
        {
            if (Enum.TryParse(typeof(TEnum), options[enumKey], true, out object enumValue))
            {
                return (TEnum)enumValue;
            }
        }
        return defaultValue;
    }

    static bool ParseOption(IDictionary<string, string> options, string option, bool defaultValue)
    {
        if (options.ContainsKey(option.ToLower()))
        {
            return bool.Parse(options[option.ToLower()]);
        }
        else
        {
            return defaultValue;
        }
    }

}


public static class EnumExtensions
{

    public static string GetDescription(this Enum anEnum)
    {
        var enumType = anEnum.GetType();
        var members = enumType.GetMember(anEnum.ToString());
        if ((members?.Length ?? 0) > 0)
        {
            var attrs = members[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((attrs?.Length ?? 0) > 0)
            {
                return ((DescriptionAttribute)attrs[0]).Description;
            }
        }
        return anEnum.ToString();
    }

    public static string ToLowerString(this Enum anEnum)
    {
        return anEnum.ToString().ToLower();
    }

}
