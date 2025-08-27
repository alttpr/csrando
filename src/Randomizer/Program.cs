using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Logging;
using Randomizer.ConsoleCommands;

var alttpr = new RootCommand("The Legend of Zelda: A Link to the Past Randomizer");
alttpr.AddCommand(new Randomize());
alttpr.AddCommand(new AssembleBaseRom());

// Global logging options
var logLevelOption = new Option<LogLevel>(
    aliases: new[] { "--log-level" },
    description: "Set minimum log level (Trace, Debug, Information, Warning, Error, Critical, None)",
    getDefaultValue: () => LogLevel.Information
);
var verboseOption = new Option<bool>(
    aliases: new[] { "-v", "--verbose" },
    description: "Enable verbose (Debug) logging"
);
alttpr.AddGlobalOption(logLevelOption);
alttpr.AddGlobalOption(verboseOption);

try
{
    // Pre-parse to set logging level before any execution output
    var parser = new Parser(alttpr);
    var parseResult = parser.Parse(args);
    var isVerbose = parseResult.GetValueForOption(verboseOption);
    var level = parseResult.GetValueForOption(logLevelOption);
    if (isVerbose && level > LogLevel.Debug)
        level = LogLevel.Debug;
    ClassLogger.SetMinimumLevel(level);

    return parser.Invoke(args);
}
finally
{
    ClassLogger.Shutdown();
}
