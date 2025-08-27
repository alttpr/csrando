# C# [VT] Randomizer

## First and foremost, big thanks to Dessyreqt, Christos, Smallhacker, and KatDevsGames for their work.
### Without their work none of this would even be remotely possible.

Further people who have been instremental in making this all work:
- Aerinon
- BhaaL
- cassidymoen
- codemann8
- Kan
- KevinCathcart
- LLCoolDave
- Orphis_Flo
- qwertymodo
- Synack
- Total
- Zarby89

(we will add more names as we think of them)

## Local Setup
Specifically for ALttP currently, please update this file as more games are added.

### System Setup
This assumes you're developing in VS Code.

## Prep a base rom
Run the following to build current base rom (from within the `src\Randomizer` directory):

```
$ dotnet run --configuration Release -- assemblebaserom --rom={path to v1.0 rom}
```

This will create an unrandomized base rom in the data folder.

## Running from the command line
To generate a game one simply runs the command (from within the `src\Randomizer` directory):

```
$ dotnet run --configuration Release -- randomize --outdir={output_directory}
```

### Logging options
The CLI supports global logging controls:

```
--log-level <Trace|Debug|Information|Warning|Error|Critical|None>
-v, --verbose   (shorthand for --log-level Debug)
```

Examples:

```
# Only warnings and errors
$ dotnet run --configuration Release -- randomize --settings data/settings.json --log-level Warning

# Verbose debug output
$ dotnet run --configuration Release -- randomize --settings data/settings.json --verbose
```

## Running tests
These can be run in VS Code. You may need to build the Tests Project first (from within the `tests\RandomizerTests` directory):

```
$ dotnet build
```

## Bug Reports
Bug reports for the current release version can be opened in this repository's [issue tracker](https://github.com/alttpr/csrando/issues).

Please do not open issues for bugs that you encounter when testing a development branch.
