namespace Randomizer;

internal static class Config
{
    static Config()
    {
        // TODO: we probably want to get this (and/or the detail paths) from a config file later
        RootDirectory = Path.Combine(Path.GetDirectoryName(typeof(Config).Assembly.Location)!, "../../../../..");

        AsmDirectory = Path.Combine(RootDirectory, "asm");
        DataDirectory = Path.Combine(RootDirectory, "data");
        BaseRomFile = Path.Combine(DataDirectory, "randomizer.sfc");
        SettingsFile = Path.Combine(DataDirectory, "settings.json");
    }
    public static string RootDirectory { get; }
    public static string AsmDirectory { get; }
    public static string DataDirectory { get; }
    public static string BaseRomFile { get; }

    public static string SettingsFile { get; }
}
