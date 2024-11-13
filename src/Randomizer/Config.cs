namespace Randomizer;

internal static class Config
{
    static Config()
    {
        // TODO: we probably want to get this (and/or the detail paths) from a config file later
        RootDirectory = Path.Combine(Path.GetDirectoryName(System.AppContext.BaseDirectory)!, "../../../../..");
        Console.WriteLine($"RootDirectory: {RootDirectory}");

        AsmDirectory = Path.Combine(RootDirectory, "asm");
        DataDirectory = Path.Combine(RootDirectory, "data");
        BaseRomFile = Path.Combine(DataDirectory, "randomizer.sfc");
        SettingsFile = Path.Combine(DataDirectory, "settings.json");
        StaticContext = new RandomizerStaticContext();
        JsonStaticContext = new RandomizerJsonStaticContext();
    }
    public static string RootDirectory { get; }
    public static string AsmDirectory { get; }
    public static string DataDirectory { get; }
    public static string BaseRomFile { get; }

    public static string SettingsFile { get; }

    public static RandomizerStaticContext StaticContext { get; }
    public static RandomizerJsonStaticContext JsonStaticContext { get; }
}
