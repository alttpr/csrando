namespace RandomizerTests.Games.Zelda1;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Randomizer.Games.Zelda1;

/// <summary>
/// Offline generator: decodes overworld walkability + entrance squares from the vanilla
/// PRG0 ROM and writes them into the committed overworld screen YAMLs (Screens/Overworld/*.yml).
///
/// This is NOT a normal test — it mutates source data files and only runs when the
/// Z1_GENERATE_OW_WALKABILITY environment variable is set, against a local ROM
/// (Z1_PRG0_ROM). The randomizer itself has no ROM at generation time, so the
/// derived walkability is committed to the repo. Re-run this if the format changes.
/// </summary>
[TestClass]
public sealed class OverworldWalkabilityGenerator
{
    private static readonly string RomPath =
        Environment.GetEnvironmentVariable("Z1_PRG0_ROM")
        ?? @"D:\Misc\xkas\multirando-asm\resources\zelda1prg0.nes";

    [TestMethod]
    public void Generate()
    {
        if (Environment.GetEnvironmentVariable("Z1_GENERATE_OW_WALKABILITY") == null)
        {
            Assert.Inconclusive("Set Z1_GENERATE_OW_WALKABILITY to run the overworld walkability generator.");
            return;
        }
        if (!File.Exists(RomPath))
        {
            Assert.Inconclusive($"PRG0 ROM not found at {RomPath}; set Z1_PRG0_ROM.");
            return;
        }

        var prg = OverworldTilemap.LoadPrg(RomPath);
        string dir = Path.Combine(FindSourceDataRoot(), "Screens", "Overworld");
        int updated = 0;

        foreach (var path in Directory.GetFiles(dir, "*.yml"))
        {
            string text = File.ReadAllText(path);
            var match = Regex.Match(text, @"(?m)^screen:\s*0x([0-9A-Fa-f]+)\s*$");
            if (!match.Success)
                continue; // Meta screen, no layout
            int screen = Convert.ToInt32(match.Groups[1].Value, 16);
            if (screen > 0x7B)
                continue;

            var walk = OverworldTilemap.DecodeWalkable(prg, screen);
            var entrances = OverworldTilemap.FindEntranceSquares(prg, screen);

            var block = BuildBlock(walk, entrances);
            string newText = InsertOrReplaceBlock(text, block);
            if (newText != text)
            {
                File.WriteAllText(path, newText);
                updated++;
            }
        }

        Console.WriteLine($"Updated {updated} overworld screen YAMLs with walkability data.");
        Assert.IsTrue(updated > 0, "Expected to update at least one screen file.");
    }

    // Walk up from the test binary to the repo's source data directory (NOT the
    // published copy under bin/), so the generator edits the committed YAMLs.
    private static string FindSourceDataRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "src", "Randomizer", "Games", "Zelda1", "data");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate src/Randomizer/Games/Zelda1/data from the test working directory.");
    }

    // Builds the YAML lines for the walkable/entrances block (LF, no trailing newline).
    private static List<string> BuildBlock(bool[,] walk, List<(int col, int row)> entrances)
    {
        var lines = new List<string> { "walkable:" };
        for (int r = 0; r < OverworldTilemap.Rows; r++)
        {
            var row = new StringBuilder();
            for (int c = 0; c < OverworldTilemap.Columns; c++)
                row.Append(walk[c, r] ? '.' : '#');
            lines.Add($"  - \"{row}\"");
        }

        string list = entrances.Count > 0
            ? string.Join(", ", entrances.Select(e => $"[{e.col}, {e.row}]"))
            : "";
        lines.Add($"entrances: [{list}]");
        return lines;
    }

    // Insert the walkable/entrances block right after the `screen:` line, replacing any previously
    // generated block. Preserves the file's existing LF endings and single trailing newline
    // (the repo enforces eol=lf via .gitattributes).
    private static string InsertOrReplaceBlock(string text, List<string> block)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
        int screenIdx = lines.FindIndex(l => Regex.IsMatch(l, @"^screen:\s*0x"));
        if (screenIdx < 0)
            return text;

        // Drop any existing walkable:/entrances:/walk-row lines following the screen line.
        int removeFrom = screenIdx + 1;
        int removeTo = removeFrom;
        while (removeTo < lines.Count &&
               (lines[removeTo].StartsWith("walkable:") ||
                lines[removeTo].StartsWith("entrances:") ||
                (lines[removeTo].StartsWith("  - \"") && LooksLikeWalkRow(lines[removeTo]))))
        {
            removeTo++;
        }
        lines.RemoveRange(removeFrom, removeTo - removeFrom);
        lines.InsertRange(screenIdx + 1, block);

        // Normalise to exactly one trailing newline.
        while (lines.Count > 0 && lines[^1].Length == 0)
            lines.RemoveAt(lines.Count - 1);
        return string.Join("\n", lines) + "\n";
    }

    private static bool LooksLikeWalkRow(string line)
    {
        var m = Regex.Match(line, "^  - \"([.#]+)\"\\s*$");
        return m.Success && m.Groups[1].Value.Length == OverworldTilemap.Columns;
    }
}
