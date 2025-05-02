using System.CommandLine;
using Randomizer.ConsoleCommands;

var alttpr = new RootCommand("The Legend of Zelda: A Link to the Past Randomizer");
alttpr.AddCommand(new Randomize());
alttpr.AddCommand(new AssembleBaseRom());
alttpr.AddCommand(new ApiServer());

try
{
    return alttpr.Invoke(args);
}
finally
{
    ClassLogger.Shutdown();
}
