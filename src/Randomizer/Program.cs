using System.CommandLine;
using Randomizer.ConsoleCommands;

var alttpr = new RootCommand("The Legend of Zelda: A Link to the Past Randomizer")
{
    new Randomize(),
    new AssembleBaseRoms(),
    new ApiServer()
};

try
{
    return alttpr.Parse(args).Invoke();
}
finally
{
    ClassLogger.Shutdown();
}
