namespace Randomizer.RomModifications;

using System.Text.RegularExpressions;
using SceneLine = (string Type, int X, int Y, string Text);

/// <summary>Class to handle Credits Sequence</summary>
public sealed class Credits
{
    private static readonly string[] _sceneOrder =
    [
        "castle",
        "sanctuary",
        "kakariko",
        "desert",
        "hera",
        "house",
        "zora",
        "witch",
        "lumberjacks",
        "grove",
        "well",
        "smithy",
        "kakariko2",
        "bridge",
        "woods",
        "pedestal",
    ];
    private readonly Dictionary<string, SceneLine[]> _scenes = new()
    {
        { "castle", [
            (Type: "small", X: 5, Y: 19, Text: "The return of the King"),
            (Type: "large", X: 9, Y: 23, Text: "Hyrule Castle"),
        ]},
        { "sanctuary", [
            (Type: "small", X: 8, Y: 19, Text: "The loyal priest"),
            (Type: "large", X: 11, Y: 23, Text: "Sanctuary"),
        ]},
        { "kakariko", [
            (Type: "small", X: 4, Y: 19, Text: "Sahasralah's Homecoming"),
            (Type: "large", X: 9, Y: 23, Text: "Kakariko Town"),
        ]},
        { "desert", [
            (Type: "small", X: 4, Y: 19, Text: "vultures rule the desert"),
            (Type: "large", X: 9, Y: 23, Text: "Desert Palace"),
        ]},
        { "hera", [
            (Type: "small", X: 4, Y: 19, Text: "the bully makes a friend"),
            (Type: "large", X: 9, Y: 23, Text: "Mountain Tower"),
        ]},
        { "house", [
            (Type: "small", X: 6, Y: 19, Text: "your uncle recovers"),
            (Type: "large", X: 11, Y: 23, Text: "Your House"),
        ]},
        { "zora", [
            (Type: "small", X: 6, Y: 19, Text: "finger webs for sale"),
            (Type: "large", X: 8, Y: 23, Text: "Zora's Waterfall"),
        ]},
        { "witch", [
            (Type: "small", X: 4, Y: 19, Text: "the witch and assistant"),
            (Type: "large", X: 11, Y: 23, Text: "Magic Shop"),
        ]},
        { "lumberjacks", [
            (Type: "small", X: 8, Y: 19, Text: "twin lumberjacks"),
            (Type: "large", X: 9, Y: 23, Text: "Woodsmen's Hut"),
        ]},
        { "grove", [
            (Type: "small", X: 4, Y: 19, Text: "ocarina boy plays again"),
            (Type: "large", X: 9, Y: 23, Text: "Haunted Grove"),
        ]},
        { "well", [
            (Type: "small", X: 4, Y: 19, Text: "venus, queen of faeries"),
            (Type: "large", X: 10, Y: 23, Text: "Wishing Well"),
        ]},
        { "smithy", [
            (Type: "small", X: 4, Y: 19, Text: "the dwarven swordsmiths"),
            (Type: "large", X: 12, Y: 23, Text: "Smithery"),
        ]},
        { "kakariko2", [
            (Type: "small", X: 6, Y: 19, Text: "the bug-catching kid"),
            (Type: "large", X: 9, Y: 23, Text: "Kakariko Town"),
        ]},
        { "bridge", [
            (Type: "small", X: 8, Y: 19, Text: "the lost old man"),
            (Type: "large", X: 9, Y: 23, Text: "Death Mountain"),
        ]},
        { "woods", [
            (Type: "small", X: 8, Y: 19, Text: "the forest thief"),
            (Type: "large", X: 11, Y: 23, Text: "Lost Woods"),
        ]},
        { "pedestal", [
            (Type: "small", X: 6, Y: 19, Text: "and the master sword"),
            (Type: "small_alt", X: 8, Y: 21, Text: "sleeps again..."),
            (Type: "large", X: 12, Y: 23, Text: "Forever!"),
        ]},
    };

    public bool UpdateCreditLine(string scene, int line, string text, string align = "center")
    {
        if (!_scenes.TryGetValue(scene, out var lineData) || lineData.Length < line)
            return false;

        ref var lineToUpdate = ref lineData[line];
        text = text.MaxLength(32);

        lineToUpdate.Text = text;

        lineToUpdate.X = align switch
        {
            "left" => 0,
            "right" => Math.Max(0, 32 - text.Length),
            // "center"
            _ => Math.Max(0, (32 - text.Length) / 2),
        };
        return true;
    }

    public (ushort[] Pointers, byte[] Data) GetBinaryData()
    {
        List<ushort> pointers = [0];
        var data = new List<byte>();
        foreach (var sceneIdentifier in _sceneOrder)
        {
            var scene = _scenes[sceneIdentifier];
            foreach (var part in scene)
            {
                switch (part.Type)
                {
                    case "small":
                        data.AddRange(GetSmallConverted(part));
                        break;
                    case "small_alt":
                        data.AddRange(GetSmallAltConverted(part));
                        break;
                    case "large":
                        data.AddRange(GetLargeConverted(part));
                        break;
                }
            }
            pointers.Add((ushort)data.Count);
        }

        return (
            Pointers: [.. pointers],
            Data: [.. data]
        );
    }

    private static byte[] GetSmallConverted(SceneLine record)
    {
        var data = new List<byte>();

        var convertedText = ConvertCredits(record.Text);
        var header = GetHeader(record.X, record.Y, convertedText.Length);
        data.AddRange(header);
        data.AddRange(convertedText);

        // special handler for apostrophes
        string apostrophes = Regex.Replace(record.Text, "[^']", " ").Replace('\'', ',');
        if (!string.IsNullOrWhiteSpace(apostrophes))
        {
            convertedText = ConvertCredits(apostrophes.Trim());
            header = GetHeader(record.X + apostrophes.IndexOf(','), record.Y - 1, convertedText.Length);
            data.AddRange(header);
            data.AddRange(convertedText);
        }

        // special handler for commas
        string commas = Regex.Replace(record.Text, "[^,]", " ").Replace(',', '\'');
        if (!string.IsNullOrWhiteSpace(commas))
        {
            convertedText = ConvertCredits(commas.Trim());
            header = GetHeader(record.X + commas.IndexOf('\''), record.Y + 1, convertedText.Length);
            data.AddRange(header);
            data.AddRange(convertedText);
        }

        return [.. data];
    }

    private static byte[] GetSmallAltConverted(SceneLine record)
    {
        var convertedText = ConvertAltCredits(record.Text);
        var header = GetHeader(record.X, record.Y, convertedText.Length);
        return [.. header, .. convertedText];
    }

    private static byte[] GetLargeConverted(SceneLine record)
    {
        var data = new List<byte>();

        var convertedText = ConvertLargeCreditsTop(record.Text);
        var header = GetHeader(record.X, record.Y, convertedText.Length);
        data.AddRange(header);
        data.AddRange(convertedText);

        convertedText = ConvertLargeCreditsBottom(record.Text);
        header = GetHeader(record.X, record.Y + 1, convertedText.Length);
        data.AddRange(header);
        data.AddRange(convertedText);

        return [.. data];
    }

    private static byte[] GetHeader(int x, int y, int length)
    {
        // NOTE: this bit twiddling already assumes little endian for the SNES.
        byte[] header = BitConverter.GetBytes((0x6000
            | y >> 5 << 11 | (y & 0x1F) << 5
            | x >> 5 << 10 | x & 0x1F) << 16
            | length * 2 - 1);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(header);

        return header;
    }

    /// <summary>Convert string to byte array for Credits that can be written to ROM</summary>
    /// @param string string string to convert
    public static byte[] ConvertLargeCreditsTop(string str)
        => str.ToLowerInvariant().Select(c => c switch
        {
            >= 'a' and <= 'z' => (byte)(c - 0x4),
            '\'' => (byte)0xD9,
            '!' => (byte)0xE5,
            '_' => (byte)0xDE,
            _ => (byte)0x9F,
        }).ToArray();

    /// <summary>
    /// Convert string to byte array for Credits that can be written to ROM
    /// </summary>
    /// <param name="str">string to convert</param>
    public static byte[] ConvertLargeCreditsBottom(string str)
        => str.ToLowerInvariant().Select(c => c switch
        {
            >= 'a' and <= 'z' => (byte)(c + 0x22),
            '\'' => (byte)0xEC,
            '!' => (byte)0xF8,
            '_' => (byte)0xF1,
            _ => (byte)0x9F,
        }).ToArray();

    /// <summary>
    /// Convert string to byte array for Credits that can be written to ROM
    /// </summary>
    /// <param name="str">string to convert</param>
    public static byte[] ConvertAltCredits(string str)
        => str.ToLowerInvariant().Select(CharToAltCreditsHex).ToArray();

    /// <summary>
    /// Convert character to byte for ROM in Credits Sequence
    /// </summary>
    /// <param name="c">character to convert</param>
    private static byte CharToAltCreditsHex(char c) => c switch
    {
        >= 'a' and <= 'z' => (byte)(c - 0x29),
        '.' => 0x52,
        _ => 0x9F,
    };

    /// <summary>
    /// Convert string to byte array for Credits that can be written to ROM
    /// </summary>
    /// <param name="str">string to convert</param>
    public static byte[] ConvertCredits(string str)
        => str.ToLowerInvariant().Select(CharToCreditsHex).ToArray();

    /// <summary>
    /// Convert character to byte for ROM in Credits Sequence
    /// </summary>
    /// <param name="c">character to convert</param>
    private static byte CharToCreditsHex(char c) => c switch
    {
        >= 'a' and <= 'z' => (byte)(c - 0x47),
        ' ' => 0x9F,
        ',' => 0x34,
        '.' => 0x37,
        '-' => 0x36,
        '\'' => 0x35,
        _ => 0x9F,
    };
}
