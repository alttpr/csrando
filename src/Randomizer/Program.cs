using System.CommandLine;
using Randomizer.ConsoleCommands;
using System.Reflection;

[assembly: AssemblyVersion("0.1.6.*")]

var alttpr = new RootCommand("The Legend of Zelda: A Link to the Past Randomizer");
alttpr.AddCommand(new Randomize());
alttpr.AddCommand(new AssembleBaseRom());

return alttpr.Invoke(args);
