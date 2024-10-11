namespace Randomizer.Games.Alttp;
/// <summary>Dialog Conversion</summary>
public sealed class Dialog
{
    /// <summary>Convert string to byte array for Dialog Box that can be written to ROM</summary>
    /// <param name="str">string to convert</param>
    /// <param name="maxBytes">maximum bytes to return</param>
    public byte[] ConvertDialog(string str, int maxBytes = 256)
    {
        var newString = new List<byte>();
        string[] lines = str.Split('\n');
        int i = 0;
        foreach (string line in lines)
        {
            switch (i)
            {
                case 0:
                    newString.Add(0x74);
                    break;
                case 1:
                    newString.Add(0x75);
                    break;
                case 2:
                default:
                    newString.Add(0x76);
                    break;
            }

            char[] lineChars = line.MaxLength(19).ToCharArray();
            if (lineChars.Length == 0)
                continue;

            foreach (char c in lineChars)
            {
                byte[] write = CharToHex(c);
                if (write[0] == 0xFD)
                {
                    newString.AddRange(write);
                }
                else
                {
                    foreach (byte b in write)
                    {
                        newString.Add(0x00);
                        newString.Add(b);
                    }
                }
            }
            if (++i % 3 == 0 && lines.Length > i)
                newString.Add(0x7E);
            if (i >= 3 && i < lines.Length)
                newString.Add(0x73);
        }

        newString.Add(0x7F);
        if (newString.Count > maxBytes)
        {
            newString[maxBytes - 1] = 0x7F;
            return [.. newString[..maxBytes]];
        }
        return [.. newString];
    }

    /// <summary>Convert string to byte array for Compressed Dialog Box that can be written to ROM</summary>
    /// <param name="str">string to convert</param>
    /// <param name="pause">whether to pause for input</param>
    /// <param name="maxBytes">maximum bytes to return</param>
    /// <param name="wrap">if greater than 0 wrap lines to this value</param>
    public byte[] ConvertDialogCompressed(string str, bool pause = true, int maxBytes = 2046, int wrap = 19)
    {
        bool padOut = false;
        List<byte> newString = [0xFB];
        string[] lines = str.Split('\n');
        if (wrap > 0)
        {
            var newLines = new List<string>();
            foreach (string line in lines)
            {
                string newLine = WordWrap(line, wrap, "\n");
                newLines.AddRange(newLine.Split("\n"));
            }
            lines = [.. newLines];
        }

        int i = 0;
        int lineCount = lines[^1].FirstOrDefault() == '{' ? lines.Length - 1 : lines.Length;
        foreach (string line in lines)
        {
            var lineChars = line.MaxLength(19).ToList();
            if (lineChars.Count == 0)
                continue;

            // command
            // @TODO: refactor this to use regex
            if (lineChars[0] == '{')
            {
                switch (line.Trim())
                {
                    case "{SPEED0}":
                        newString.AddRange([0xFC, 0x00]);
                        break;
                    case "{SPEED2}":
                        newString.AddRange([0xFC, 0x02]);
                        break;
                    case "{SPEED6}":
                        newString.AddRange([0xFC, 0x06]);
                        break;
                    case "{PAUSE1}":
                        newString.AddRange([0xFE, 0x78, 0x01]);
                        break;
                    case "{PAUSE3}":
                        newString.AddRange([0xFE, 0x78, 0x03]);
                        break;
                    case "{PAUSE5}":
                        newString.AddRange([0xFE, 0x78, 0x05]);
                        break;
                    case "{PAUSE7}":
                        newString.AddRange([0xFE, 0x78, 0x07]);
                        break;
                    case "{PAUSE9}":
                        newString.AddRange([0xFE, 0x78, 0x09]);
                        break;
                    case "{INPUT}":
                        newString.AddRange([0xFA]);
                        break;
                    case "{CHOICE}":
                        newString.AddRange([0xFE, 0x68]);
                        break;
                    case "{ITEMSELECT}":
                        newString.AddRange([0xFE, 0x69]);
                        break;
                    case "{CHOICE2}":
                        newString.AddRange([0xFE, 0x71]);
                        break;
                    case "{CHOICE3}":
                        newString.AddRange([0xFE, 0x72]);
                        break;
                    case "{C:GREEN}":
                        newString.AddRange([0xFE, 0x77, 0x07]);
                        break;
                    case "{C:YELLOW}":
                        newString.AddRange([0xFE, 0x77, 0x02]);
                        break;
                    case "{HARP}":
                        newString.AddRange([0xFE, 0x79, 0x2D]);
                        break;
                    case "{MENU}":
                        newString.AddRange([0xFE, 0x6D, 0x00]);
                        break;
                    case "{BOTTOM}":
                        newString.AddRange([0xFE, 0x6D, 0x01]);
                        break;
                    case "{NOBORDER}":
                        newString.AddRange([0xFE, 0x6B, 0x02]);
                        break;
                    case "{CHANGEPIC}":
                        newString.AddRange([0xFE, 0x67, 0xFE, 0x67]);
                        break;
                    case "{CHANGEMUSIC}":
                        newString.AddRange([0xFE, 0x67]);
                        break;
                    case "{INTRO}":
                        padOut = true;
                        newString.AddRange([0xFE, 0x6E, 0x00, 0xFE, 0x77, 0x07, 0xFC, 0x03, 0xFE, 0x6B, 0x02, 0xFE, 0x67]);
                        break;
                    case "{NOTEXT}":
                        return [0xFB, 0xFE, 0x6E, 0x00, 0xFE, 0x6B, 0x04];
                    case "{IBOX}":
                        newString.AddRange([0xFE, 0x6B, 0x02, 0xFE, 0x77, 0x07, 0xFC, 0x03, 0xF7]);
                        break;
                }
                lineCount--;
                if (newString.Count > maxBytes)
                    throw new Exception("command overflowed byte length");

                continue;
            }

            switch (i)
            {
                case 0:
                    break;
                case 1:
                    newString.Add(0xF8); // row 2
                    break;
                case 2:
                default:
                    if (i >= 3 && i < lines.Length)
                        newString.Add(0xF6); // scroll
                    else
                        newString.Add(0xF9); // row 3
                    break;
            }

            // the first box needs to fill the full width with spaces as the palette is loaded weird.
            if (padOut && i < 3)
                lineChars.AddRange(Enumerable.Repeat(' ', 19 - lineChars.Count));

            foreach (char c in lineChars)
                newString.AddRange(CharToHex(c));

            i++;

            if (pause && i % 3 == 0 && lineCount > i)
                newString.Add(0xFA); // wait for input
        }

        if (newString.Count > maxBytes)
            return [.. newString[..maxBytes]];

        return [.. newString];
    }

    private static string WordWrap(string str, int width = 75, string separator = "\n", bool cut = false)
    {
        string[] lines = str.Split(separator);

        if (lines.Length == 0)
            return str;

        var wrapped = new List<string>();
        foreach (string l in lines)
        {
            string line = l.TrimEnd();
            if (line.Length <= width)
            {
                wrapped.Add(line);
                continue;
            }
            string[] words = line.Split(' ');
            string actual = "";
            foreach (string word in words)
            {
                if ((actual + word).Length <= width)
                {
                    actual += word + ' ';
                }
                else
                {
                    if (actual.Length > 0)
                    {
                        wrapped.Add(actual.TrimEnd());
                    }
                    actual = word;
                    if (cut)
                    {
                        while (actual.Length > width)
                        {
                            wrapped.Add(actual[..width]);
                            actual = actual[width..];
                        }
                    }
                    actual += ' ';
                }
            }
            wrapped.Add(actual.Trim());
        }
        return string.Join(separator, wrapped);
    }

    private static readonly Dictionary<char, byte[]> _characters = new()
    {
        { ' ', [0xFF] },
        { '≥', [0x99] }, // cursor
        { '…', [0x9F] },
        { '?', [0xC6] },
        { '!', [0xC7] },
        { ',', [0xC8] },
        { '-', [0xC9] },
        { '.', [0xCD] },
        { '~', [0xCE] },
        { '～', [0xCE] },
        { ':', [0xEA] },
        { '\'', [0x9D] },
        { '’', [0x9D] },
        { '@', [0xFE, 0x6A] }, // link's name compressed
        { '>', [0x9B, 0x9C] }, // link face
        { '%', [0xFD, 0x10] }, // Hylian Bird
        { '^', [0xFD, 0x11] }, // Hylian Ankh
        { '=', [0xFD, 0x12] }, // Hylian Wavy lines
        { '↑', [0xFD, 0x13] },
        { '↓', [0xFD, 0x14] },
        { '→', [0xFD, 0x15] },
        { '←', [0xFD, 0x16] },
        { '¼', [0xE5, 0xE7] }, // ¼ heart
        { '½', [0xE6, 0xE7] }, // ½ heart
        { '¾', [0xE8, 0xE9] }, // ¾ heart
        { '♥', [0xEA, 0xEB] }, // full heart
        { 'ᚋ', [0xFE, 0x6C, 0x00] }, // var 0
        { 'ᚌ', [0xFE, 0x6C, 0x01] }, // var 1
        { 'ᚍ', [0xFE, 0x6C, 0x02] }, // var 2
        { 'ᚎ', [0xFE, 0x6C, 0x03] }, // var 3
        { 'あ', [0x00] },
        { 'い', [0x01] },
        { 'う', [0x02] },
        { 'え', [0x03] },
        { 'お', [0x04] },
        { 'や', [0x05] },
        { 'ゆ', [0x06] },
        { 'よ', [0x07] },
        { 'か', [0x08] },
        { 'き', [0x09] },
        { 'く', [0x0A] },
        { 'け', [0x0B] },
        { 'こ', [0x0C] },
        { 'わ', [0x0D] },
        { 'を', [0x0E] },
        { 'ん', [0x0F] },
        { 'さ', [0x10] },
        { 'し', [0x11] },
        { 'す', [0x12] },
        { 'せ', [0x13] },
        { 'そ', [0x14] },
        { 'が', [0x15] },
        { 'ぎ', [0x16] },
        { 'ぐ', [0x17] },
        { 'た', [0x18] },
        { 'ち', [0x19] },
        { 'つ', [0x1A] },
        { 'て', [0x1B] },
        { 'と', [0x1C] },
        { 'げ', [0x1D] },
        { 'ご', [0x1E] },
        { 'ざ', [0x1F] },
        { 'な', [0x20] },
        { 'に', [0x21] },
        { 'ぬ', [0x22] },
        { 'ね', [0x23] },
        { 'の', [0x24] },
        { 'じ', [0x25] },
        { 'ず', [0x26] },
        { 'ぜ', [0x27] },
        { 'は', [0x28] },
        { 'ひ', [0x29] },
        { 'ふ', [0x2A] },
        { 'へ', [0x2B] },
        { 'ほ', [0x2C] },
        { 'ぞ', [0x2D] },
        { 'だ', [0x2E] },
        { 'ぢ', [0x2F] },
        { 'ま', [0x30] },
        { 'み', [0x31] },
        { 'む', [0x32] },
        { 'め', [0x33] },
        { 'も', [0x34] },
        { 'づ', [0x35] },
        { 'で', [0x36] },
        { 'ど', [0x37] },
        { 'ら', [0x38] },
        { 'り', [0x39] },
        { 'る', [0x3A] },
        { 'れ', [0x3B] },
        { 'ろ', [0x3C] },
        { 'ば', [0x3D] },
        { 'び', [0x3E] },
        { 'ぶ', [0x3F] },
        { 'べ', [0x40] },
        { 'ぼ', [0x41] },
        { 'ぱ', [0x42] },
        { 'ぴ', [0x43] },
        { 'ぷ', [0x44] },
        { 'ぺ', [0x45] },
        { 'ぽ', [0x46] },
        { 'ゃ', [0x47] },
        { 'ゅ', [0x48] },
        { 'ょ', [0x49] },
        { 'っ', [0x4A] },
        { 'ぁ', [0x4B] },
        { 'ぃ', [0x4C] },
        { 'ぅ', [0x4D] },
        { 'ぇ', [0x4E] },
        { 'ぉ', [0x4F] },
        { 'ア', [0x50] },
        { 'イ', [0x51] },
        { 'ウ', [0x52] },
        { 'エ', [0x53] },
        { 'オ', [0x54] },
        { 'ヤ', [0x55] },
        { 'ユ', [0x56] },
        { 'ヨ', [0x57] },
        { 'カ', [0x58] },
        { 'キ', [0x59] },
        { 'ク', [0x5A] },
        { 'ケ', [0x5B] },
        { 'コ', [0x5C] },
        { 'ワ', [0x5D] },
        { 'ヲ', [0x5E] },
        { 'ン', [0x5F] },
        { 'サ', [0x60] },
        { 'シ', [0x61] },
        { 'ス', [0x62] },
        { 'セ', [0x63] },
        { 'ソ', [0x64] },
        { 'ガ', [0x65] },
        { 'ギ', [0x66] },
        { 'グ', [0x67] },
        { 'タ', [0x68] },
        { 'チ', [0x69] },
        { 'ツ', [0x6A] },
        { 'テ', [0x6B] },
        { 'ト', [0x6C] },
        { 'ゲ', [0x6D] },
        { 'ゴ', [0x6E] },
        { 'ザ', [0x6F] },
        { 'ナ', [0x70] },
        { 'ニ', [0x71] },
        { 'ヌ', [0x72] },
        { 'ネ', [0x73] },
        { 'ノ', [0x74] },
        { 'ジ', [0x75] },
        { 'ズ', [0x76] },
        { 'ゼ', [0x77] },
        { 'ハ', [0x78] },
        { 'ヒ', [0x79] },
        { 'フ', [0x7A] },
        { 'ヘ', [0x7B] },
        { 'ホ', [0x7C] },
        { 'ゾ', [0x7D] },
        { 'ダ', [0x7E] },
        { 'マ', [0x80] },
        { 'ミ', [0x81] },
        { 'ム', [0x82] },
        { 'メ', [0x83] },
        { 'モ', [0x84] },
        { 'ヅ', [0x85] },
        { 'デ', [0x86] },
        { 'ド', [0x87] },
        { 'ラ', [0x88] },
        { 'リ', [0x89] },
        { 'ル', [0x8A] },
        { 'レ', [0x8B] },
        { 'ロ', [0x8C] },
        { 'バ', [0x8D] },
        { 'ビ', [0x8E] },
        { 'ブ', [0x8F] },
        { 'ベ', [0x90] },
        { 'ボ', [0x91] },
        { 'パ', [0x92] },
        { 'ピ', [0x93] },
        { 'プ', [0x94] },
        { 'ペ', [0x95] },
        { 'ポ', [0x96] },
        { 'ャ', [0x97] },
        { 'ュ', [0x98] },
        { 'ョ', [0x99] },
        { 'ッ', [0x9A] },
        { 'ァ', [0x9B] },
        { 'ィ', [0x9C] },
        { 'ゥ', [0x9D] },
        { 'ェ', [0x9E] },
        { 'ォ', [0x9F] },
    };

    /// <summary>Convert character to byte for ROM</summary>
    /// <param name="c">character to convert</param>
    private static byte[] CharToHex(char c) => c switch
    {
        >= '0' and <= '9' => [(byte)(c - '0' + 0xA0)],
        >= 'A' and <= 'Z' => [(byte)(c - 'A' + 0xAA)],
        >= 'a' and <= 'z' => [(byte)(c + 0x6F)],
        _ => _characters.GetValueOrDefault(c, [0xFF])
    };
}
