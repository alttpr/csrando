using System.CommandLine;
using Randomizer.ConsoleCommands;

var alttpr = new RootCommand("The Legend of Zelda: A Link to the Past Randomizer");
alttpr.AddCommand(new Randomize());

return alttpr.Invoke(args);
