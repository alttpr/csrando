using System.CommandLine;
using Randomizer.Console.Commands;

var alttpr = new RootCommand("The Legend of Zelda: A Link to the Past Randomizer");
alttpr.AddCommand(new Randomize());

return alttpr.Invoke(args);
