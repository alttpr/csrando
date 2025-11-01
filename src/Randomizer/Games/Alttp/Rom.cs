namespace Randomizer.Games.Alttp;

using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using Randomizer.Games;
using Randomizer.Graph;
using Randomizer.RomModifications;

public sealed class Rom : GameRom
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private const byte NOP = 0xEA;

    private readonly Text _text;
    private readonly Credits _credits;
    private readonly YamlReader.GameData _gameData;

    internal InitialSram InitialSram { get; }

    public Rom(RomModifications.IRom rom, string language, int offset)
        : base(rom, offset)
    {
        InitialSram = new();
        _text = new(language);
        _text.RemoveUnwanted();
        _credits = new();
        _gameData = YamlReader.LoadGameData();
    }

    /// <summary>Write subsitutions</summary>
    /// <param name="substitutions">[[id, max, replace id, 0xFF], ...]</param>
    public void SetSubstitutions(in ReadOnlySpan<byte> substitutions)
        => Write((SNES)0xB0C000, [.. substitutions, 0xFF, 0xFF, 0xFF, 0xFF]);

    /// <summary>Set the Low Health Beep Speed</summary>
    /// <param name="setting">name (0x00: off, 0x20: normal, 0x40: half, 0x80: quarter)</param>
    public void SetHeartBeepSpeed(HeartBeepSpeedOption setting)
        => Write((SNES)0xB08033, [(byte)setting]);

    /// <summary>Set the Rupoor value to take rupees</summary>
    public void SetRupoorValue(ushort value = 10)
    {
        Span<byte> data = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(data, value);
        Write((SNES)0xB08036, data);
    }

    /// <summary>Set Cane of Byrna Cave spike floor damage</summary>
    /// <param name="damageValue">(0x08: 1 Heart, 0x02: 1/4 Heart)</param>
    // TODO: don't use a byte for this?
    public void SetByrnaCaveSpikeDamage(byte damageValue = 0x08)
        => Write((SNES)0xB08195, [damageValue]);

    /// <summary>Set Cane of Byrna Cave and Misery Mire spike room Byrna usage</summary>
    /// <param name="normal">normal magic usage</param>
    /// <param name="half">half magic usage</param>
    /// <param name="quarter">quarter magic usage</param>
    // TODO: don't use bytes for this?
    public void SetCaneOfByrnaSpikeCaveUsage(byte normal = 0x04, byte half = 0x02, byte quarter = 0x01)
        => Write((SNES)0xB0816B, [normal, half, quarter]);

    /// <summary>Enable Byrna"s ability to make you Invulnerable</summary>
    /// <param name="enable">switch on or off</param>
    public void SetCaneOfByrnaInvulnerability(bool enable = true)
        => Write((SNES)0xB0804F, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Bryna magic amount used per "cycle"</summary>
    /// <param name="normal">normal magic usage</param>
    /// <param name="half">half magic usage</param>
    /// <param name="quarter">quarter magic usage</param>
    public void SetCaneOfByrnaMagicPerCycle(byte normal = 0x04, byte half = 0x02, byte quarter = 0x01)
        => Write((SNES)0x08DC42, [normal, half, quarter]);

    /// <summary>Set Cane of Byrna Cave and Misery Mire spike room Cape usage</summary>
    /// <param name="normal">normal magic usage</param>
    /// <param name="half">half magic usage</param>
    /// <param name="quarter">quarter magic usage</param>
    // TODO: don't use bytes for this?
    public void SetCapeSpikeCaveUsage(byte normal = 0x04, byte half = 0x08, byte quarter = 0x10)
        => Write((SNES)0xB0816E, [normal, half, quarter]);

    /// <summary>Set regular Cape Magic Usage</summary>
    /// <param name="normal">normal magic usage</param>
    /// <param name="half">half magic usage</param>
    /// <param name="quarter">quarter magic usage</param>
    // TODO: don't use bytes for this?
    public void SetCapeRegularMagicUsage(byte normal = 0x04, byte half = 0x08, byte quarter = 0x10)
        => Write((SNES)0x07ADA7, [normal, half, quarter]);

    /// <summary>Set mode for HUD clock</summary>
    /// <param name="mode">off|stopwatch|countdown-stop|countdown-continue</param>
    /// <param name="restart">whether to restart the timer</param>
    // TODO: don't use a string for this
    public void SetClockMode(string mode = "off", bool restart = false)
    {
        bool compassOverride = true;
        byte[] bytes;
        switch (mode)
        {
            case "stopwatch":
                bytes = [0x02, 0x01];
                break;
            case "countdown-ohko":
                bytes = [0x01, 0x02];
                restart = true;
                break;
            case "countdown-continue":
                bytes = [0x01, 0x01];
                break;
            case "countdown-stop":
                bytes = [0x01, 0x00];
                break;
            case "countdown-end":
                bytes = [0x01, 0x03];
                restart = false;
                break;
            case "off":
            default:
                bytes = [0x00, 0x00];
                compassOverride = false;
                break;
        }

        // TODO: temporarily disable compass mode while this is enabled since they occupy the same region of the hud.
        if (compassOverride)
            SetCompassMode(CompassCounterOption.Off);

        Write((SNES)0xB08190, [.. bytes, (byte)(restart ? 0x01 : 0x00)]);
    }

    /// <summary>Enable triforce-hunt turn in mode.</summary>
    /// <param name="enable">enable or disable turn in mode.</param>
    public void EnableTriforceTurnIn(bool enable = true)
        => Write((SNES)0xB08194, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable HUD item counter</summary>
    /// <param name="enable">enable or disable collection count / total item count on HUD</param>
    public void EnableHudItemCounter(bool enable = false)
        => Write((SNES)0xB08039, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Set starting time for HUD clock.</summary>
    /// <param name="seconds">time in seconds;</param>
    public void SetStartingTime(int seconds = 0)
    {
        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(data, seconds * 60);
        Write((SNES)0xB0820C, data);
    }

    /// <summary>Set time adjustment for collecting Red Clock Item</summary>
    /// <param name="seconds">time in seconds;</param>
    public void SetRedClock(int seconds = 0)
    {
        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(data, seconds * 60);
        Write((SNES)0xB08200, data);
    }

    /// <summary>Set time adjustment for collecting Blue Clock Item</summary>
    /// <param name="seconds">time in seconds;</param>
    public void SetBlueClock(int seconds = 0)
    {
        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(data, seconds * 60);
        Write((SNES)0xB08204, data);
    }

    /// <summary>Set time adjustment for collecting Green Clock Item</summary>
    /// <param name="seconds">time in seconds;</param>
    public void SetGreenClock(int seconds = 0)
    {
        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(data, seconds * 60);
        Write((SNES)0xB08208, data);
    }

    /// <summary>Set the Digging Game Rng</summary>
    public void SetDiggingGameRng(byte digs = 15)
    {
        Write((SNES)0xB08020, [digs]);
        Write((SNES)0x1DFD95, [digs]);
    }

    /// <summary>Set values to fill for Capacity Upgrades</summary>
    public void SetCapacityUpgradeFills(byte bomb5, byte bomb10, byte arrow5, byte arrow10)
        => Write((SNES)0xB08080, [bomb5, bomb10, arrow5, arrow10]);

    /// <summary>Set values to fill for Health/Magic fills from Bottles</summary>
    public void SetBottleFills(byte health, byte magic)
        => Write((SNES)0xB08084, [health, magic]);

    /// <summary>Set the number of goal items to collect</summary>
    public void SetGoalRequiredCount(ushort goal = 0)
    {
        Span<byte> data = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(data, goal);
        Write((SNES)0xB08167, data);
    }

    /// <summary>Set the goal item icon</summary>
    public void SetGoalIcon(GoalIconOption goalIcon = GoalIconOption.Triforce)
    {
        ReadOnlySpan<byte> bytes = goalIcon switch
        {
            GoalIconOption.Triforce => [0x0E, 0x28],
            // GoalIconOption.Star
            _ => [0x0D, 0x28],
        };
        Write((SNES)0xB08165, bytes);
    }

    /// <summary>Set Progressive Sword limit and item after limit is reached</summary>
    /// <param name="limit">max number to receive</param>
    /// <param name="item">item byte to collect once limit is collected</param>
    public void SetLimitProgressiveSword(byte limit = 4, byte item = 0x36)
        => Write((SNES)0xB08090, [limit, item]);

    /// <summary>Set Progressive Shield limit and item after limit is reached</summary>
    /// <param name="limit">max number to receive</param>
    /// <param name="item">item byte to collect once limit is collected</param>
    public void SetLimitProgressiveShield(byte limit = 3, byte item = 0x36)
        => Write((SNES)0xB08092, [limit, item]);

    /// <summary>Set Progressive Armor limit and item after limit is reached</summary>
    /// <param name="limit">max number to receive</param>
    /// <param name="item">item byte to collect once limit is collected</param>
    public void SetLimitProgressiveArmor(byte limit = 2, byte item = 0x36)
        => Write((SNES)0xB08094, [limit, item]);

    /// <summary>Set Bottle limit and item after limit is reached</summary>
    /// <param name="limit">max number to receive</param>
    /// <param name="item">item byte to collect once limit is collected</param>
    public void SetLimitBottle(byte limit = 4, byte item = 0x36)
        => Write((SNES)0xB08096, [limit, item]);

    /// <summary>Set Progressive Bow limit and item after limit is reached</summary>
    /// <param name="limit">max number to receive</param>
    /// <param name="item">item byte to collect once limit is collected</param>
    public void SetLimitProgressiveBow(byte limit = 2, byte item = 0x36)
        => Write((SNES)0xB08098, [limit, item]);

    /// <summary>
    /// Set Ganon to Invincible. 'dungeons' will require all dungeon bosses are dead to be able to damage Ganon.
    /// </summary>
    // TODO: don't use a string for this
    public void SetGanonInvincible(string setting = "no")
    {
        var b = setting switch
        {
            "crystals" => (byte)0x03,
            "dungeons" => (byte)0x02,
            "yes" => (byte)0x01,
            "crystals_only" => (byte)0x04,
            "triforce_pieces" => (byte)0x05,
            // light world only, pull ped, kill aga 1
            "lightspeed" => (byte)0x06,
            "crystals_bosses" => (byte)0x07,
            "bosses_only" => (byte)0x08,
            // all dungeons, aga 1 not required
            "dungeons_no_agahnim" => (byte)0x09,
            // 100% collection rate, all dungeons
            "completionist" => (byte)0x0B,
            // "no"
            _ => (byte)0x00,
        };

        Write((SNES)0xB081A8, [b]);
    }

    /// <summary>Set hearts color for low vision people</summary>
    /// <param name="color">color to have HUD hearts</param>
    public void SetHeartColors(HeartColorOption color)
    {
        var b = color switch
        {
            HeartColorOption.Blue => (byte)0x01,
            HeartColorOption.Green => (byte)0x02,
            HeartColorOption.Yellow => (byte)0x03,
            // HeartColorOption.Red
            _ => (byte)0x00,
        };
        Write((SNES)0xB0F020, [b]);
    }

    /// <summary>Set the specified text to a custom value</summary>
    /// <param name="key">which text to set</param>
    /// <param name="string">text to display</param>
    public void SetText(string key, string str, bool pause = true, int maxBytes = 2046, int wrap = 19)
        => _text.SetString(key, str, pause, maxBytes, wrap);

    /// <summary>Commit the text table to ROM</summary>
    public void WriteText()
        => Write((SNES)0x1C8000, _text.GetByteArray());

    /// <summary>Set the specified credits line to a custom value</summary>
    /// <param name="key">   which credit to set</param>
    /// <param name="string">text to display</param>
    public void SetCredit(string key, string str)
        => _credits.UpdateCreditLine(key, 0, str);

    /// <summary>Write the credits sequnce</summary>
    public void WriteCredits()
    {
        var (pointers, data) = _credits.GetBinaryData();

        Write((SNES)0xB09500, data);
        Span<byte> p = stackalloc byte[pointers.Length * 2];
        var dataP = p;
        foreach (ushort pointer in pointers)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(dataP, pointer);
            dataP = dataP[2..];
        }
        Write((SNES)0x0EECC0, p);
    }

    /// <summary>Set Menu Speed</summary>
    /// <param name="menuSpeed">speed at which the menu enters the screen</param>
    public void SetMenuSpeed(MenuSpeedOption menuSpeed = MenuSpeedOption.Normal)
    {
        bool fast = menuSpeed == MenuSpeedOption.Instant;
        byte speed = (byte)menuSpeed;
        Write((SNES)0xB08048, [speed]);
        Write((SNES)0x0DDD9A, [(byte)(fast ? 0x20 : 0x11)]);
        Write((SNES)0x0DDF2A, [(byte)(fast ? 0x20 : 0x12)]);
        Write((SNES)0x0DE0E9, [(byte)(fast ? 0x20 : 0x12)]);
    }

    /// <summary>Enable/Disable the Quickswap void</summary>
    /// <param name="enable">switch on or off</param>
    public void SetQuickSwap(bool enable = false)
        => Write((SNES)0xB0804B, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable/Disable the Smithy Full Travel void</summary>
    /// <param name="enable">switch on or off</param>
    public void SetSmithyFreeTravel(bool enable = false)
        => Write((SNES)0xB0804C, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Set Programmable 1 item.</summary>
    public void SetProgrammable1(string custom)
    {
        switch (custom)
        {
            case "bees":
                Write((SNES)0xBB8000, [
                    0xA9,
                    0x79,
                    0x22,
                    0x5D,
                    0xF6,
                    0x1D,
                    0x30,
                    0x14,
                    0xA5,
                    0x22,
                    0x99,
                    0x10,
                    0x0D,
                    0xA5,
                    0x23,
                    0x99,
                    0x30,
                    0x0D,
                    0xA5,
                    0x20,
                    0x99,
                    0x00,
                    0x0D,
                    0xA5,
                    0x21,
                    0x99,
                    0x20,
                    0x0D,
                    0x6B
                ]);
                Write((SNES)0xB08061, [0x00, 0x80, 0x3B]);

                break;
        }
    }

    /// <summary>Set the Seed Type</summary>
    /// <param name="setting">name</param>
    // TODO: don't use a string for this
    public void SetRandomizerSeedType(string setting)
    {
        var b = setting switch
        {
            "OverworldGlitches" => (byte)0x02,
            "MajorGlitches" or "HybridMajorGlitches" => (byte)0x01,
            "off" => (byte)0xFF,
            // "NoGlitches"
            _ => (byte)0x00,
        };
        Write((SNES)0xB08210, [b]);
    }

    /// <summary>Set the Game Type</summary>
    /// <param name="setting">name</param>
    // TODO: don't use a string for this
    public void SetGameType(string setting)
    {
        var b = setting switch
        {
            "enemizer" => (byte)0b0000_0101,
            "entrance" => (byte)0b0000_0110,
            "room" => (byte)0b0000_1000,
            // "item"
            _ => (byte)0b0000_0100,
        };
        Write((SNES)0xB08211, [b]);
    }

    /// <summary>Set the Plandomizer Author</summary>
    /// <param name="name">name of author</param>
    public void SetPlandomizerAuthor(string name)
        => Write((SNES)0xB08220, [.. name.MaxLength(31).Select(c => (byte)c)]);

    /// <summary>Set the Tournament Type</summary>
    /// <param name="setting">name</param>
    // TODO: don't use a string for this?
    public void SetTournamentType(string setting)
    {
        byte[] bytes = setting switch
        {
            "standard" => [0x01, 0x00],
            // "none"
            _ => [0x00, 0x01],
        };
        Write((SNES)0xB08213, bytes);
    }

    /// <summary>Set the Hash on the Start Screen</summary>
    /// <param name="bytes">5 bytes that will appear on the start screen for verification</param>
    public void SetStartScreenHash(byte[] bytes)
    {
        int oldLength = bytes.Length;
        Array.Resize(ref bytes, 5);
        if (oldLength < 5)
            Array.Fill(bytes, (byte)0, oldLength, 5 - oldLength);
        Write((SNES)0xB08215, bytes);
    }

    /// <summary>Removes Shield from Uncle by moving the tiles for shield to his head and replaces them with his head.</summary>
    public void RemoveUnclesShield()
    {
        Write((SNES)0x0DD253, [0x00, 0x00, 0xf6, 0xff, 0x00, 0x0E]);
        Write((SNES)0x0DD25B, [0x00, 0x00, 0xf6, 0xff, 0x00, 0x0E]);
        Write((SNES)0x0DD283, [0x00, 0x00, 0xf6, 0xff, 0x00, 0x0E]);
        Write((SNES)0x0DD28B, [0x00, 0x00, 0xf7, 0xff, 0x00, 0x0E]);
        Write((SNES)0x0DD2CB, [0x00, 0x00, 0xf6, 0xff, 0x02, 0x0E]);
        Write((SNES)0x0DD2FB, [0x00, 0x00, 0xf7, 0xff, 0x02, 0x0E]);
        Write((SNES)0x0DD313, [0x00, 0x00, 0xe4, 0xff, 0x08, 0x0E]);
    }

    /// <summary>Removes Sword from Uncle by moving the tiles for sword to his head and replaces them with his head.</summary>
    public void RemoveUnclesSword()
    {
        Write((SNES)0x0DD263, [0x00, 0x00, 0xf6, 0xff, 0x00, 0x0E]);
        Write((SNES)0x0DD26B, [0x00, 0x00, 0xf6, 0xff, 0x00, 0x0E]);
        Write((SNES)0x0DD293, [0x00, 0x00, 0xf6, 0xff, 0x00, 0x0E]);
        Write((SNES)0x0DD29B, [0x00, 0x00, 0xf7, 0xff, 0x00, 0x0E]);
        Write((SNES)0x0DD2B3, [0x00, 0x00, 0xf6, 0xff, 0x02, 0x0E]);
        Write((SNES)0x0DD2BB, [0x00, 0x00, 0xf6, 0xff, 0x02, 0x0E]);
        Write((SNES)0x0DD2E3, [0x00, 0x00, 0xf7, 0xff, 0x02, 0x0E]);
        Write((SNES)0x0DD2EB, [0x00, 0x00, 0xf7, 0xff, 0x02, 0x0E]);
        Write((SNES)0x0DD31B, [0x00, 0x00, 0xe4, 0xff, 0x08, 0x0E]);
        Write((SNES)0x0DD323, [0x00, 0x00, 0xe4, 0xff, 0x08, 0x0E]);
    }

    /// <summary>Set the sprite that spawns when powdered sprite that usually spawns a faerie is powdered.</summary>
    /// <param name="sprite">id of sprite to drop</param>
    public void SetPowderedSpriteFairyPrize(byte sprite = 0xE3)
        => Write((SNES)0x06EDD0, [sprite]);

    /// <summary>Set Overworld bonk prizes</summary>
    /// <param name="prizes">ids of sprites to drop (0x03 empty)</param>
    public void SetOverworldBonkPrizes(byte[]? prizes = null)
    {
        prizes ??= [];
        SNES[] addresses = [
            (SNES)0x09CF6C,
            (SNES)0x09CFBA,
            (SNES)0x09CFE0,
            (SNES)0x09CFFB,
            (SNES)0x09D018,
            (SNES)0x09D01B,
            (SNES)0x09D028,
            (SNES)0x09D03C,
            (SNES)0x09D059,
            (SNES)0x09D07A,
            (SNES)0x09D09E,
            (SNES)0x09D0A8,
            (SNES)0x09D0AB,
            (SNES)0x09D0AE,
            (SNES)0x09D0BE,
            (SNES)0x09D0DD,
            (SNES)0x09D16A,
            (SNES)0x09D1E5,
            (SNES)0x09D1EE,
            (SNES)0x09D20B,
            (SNES)0x09CBBF,
            (SNES)0x09CBBF,
            (SNES)0x09CC17,
            (SNES)0x09CC1A,
            (SNES)0x09CC4A,
            (SNES)0x09CC4D,
            (SNES)0x09CC53,
            (SNES)0x09CC69,
            (SNES)0x09CC6F,
            (SNES)0x09CC7C,
            (SNES)0x09CCEF,
            (SNES)0x09CD51,
            (SNES)0x09CDC0,
            (SNES)0x09CDC3,
            (SNES)0x09CDC6,
            (SNES)0x09CE37,
            (SNES)0x09D2DE,
            (SNES)0x09D32F,
            (SNES)0x09D355,
            (SNES)0x09D367,
            (SNES)0x09D384,
            (SNES)0x09D387,
            (SNES)0x09D397,
            (SNES)0x09D39E,
            (SNES)0x09D3AB,
            (SNES)0x09D3AE,
            (SNES)0x09D3D1,
            (SNES)0x09D3D7,
            (SNES)0x09D3F8,
            (SNES)0x09D416,
            (SNES)0x09D420,
            (SNES)0x09D423,
            (SNES)0x09D42D,
            (SNES)0x09D449,
            (SNES)0x09D48C,
            (SNES)0x09D4D9,
            (SNES)0x09D4DC,
            (SNES)0x09D4E3,
            (SNES)0x09D504,
            (SNES)0x09D507,
            (SNES)0x09D55E,
            (SNES)0x09D56A,
        ];

        int prizeIdx = 0;
        foreach (var address in addresses)
        {
            byte item = prizeIdx < prizes.Length ? prizes[prizeIdx++] : (byte)0x03;
            Write(address, [item]);
        }
    }

    /// <summary>Set Overworld dig prizes.</summary>
    /// <param name="prizes">ids of sprites to dig up, 64 bytes</param>
    public void SetOverworldDigPrizes(byte[]? prizes = null)
    {
        prizes ??= [];
        if (prizes.Length > 64)
        {
            prizes = prizes[..64];
        }
        else if (prizes.Length < 64)
        {
            int oldLength = prizes.Length;
            Array.Resize(ref prizes, 64);
            // pad with green rupees, until we find something more suitable.
            Array.Fill(prizes, (byte)0x34, oldLength, prizes.Length - oldLength);
        }

        Write((SNES)0xB08100, prizes);
    }

    // FIXME: temporary classes until we have something usable from elsewhere
    public sealed class Shop
    {
        public bool Active { get; }
        // 1 for TakeAny caves (can only get one item, not both), 3 for regular shops
        public int ObtainableInventorySize { get; }
        public Shop() => throw new NotImplementedException("This is not a proper shop, but a placeholder.");
        public void WriteExtraData(Rom rom) => throw new NotImplementedException();
        public byte[] GetBytes(int sramOffset) => throw new NotImplementedException();
        public IEnumerable<ShopItem> GetInventory() => throw new NotImplementedException();
    }
    public sealed class ShopItem
    {
        public byte Id { get; }
        public ushort Price { get; }
        public byte Max { get; }
        public byte ReplaceId { get; } = 0xFF;
        public ushort ReplacePrice { get; }
        public ShopItem() => throw new NotImplementedException("This is not a proper shop item, but a placeholder.");
    }
    /// <summary>Quick and dirty shop setting code.</summary>
    /// <param name="shops">shops to write to ROM</param>
    public void SetupCustomShops(Shop[] shops)
    {
        shops = shops.Where(s => s.Active).ToArray();

        var shopData = new List<byte>();
        var itemsData = new List<byte>();
        byte shopId = 0x00;
        int sramOffset = 0x00;
        foreach (var shop in shops)
        {
            if (shopId == shops.Length - 1)
                shopId = 0xFF;

            shop.WriteExtraData(this);
            // TODO: make this clever and reuse when inv is the exact same. (except take any's)
            shopData.Add(shopId);
            shopData.AddRange(shop.GetBytes(sramOffset));
            sramOffset += shop.ObtainableInventorySize;

            if (sramOffset > 36)
                throw new Exception("Exceeded SRAM indexing for shops");

            foreach (var item in shop.GetInventory())
            {
                itemsData.Add(shopId);
                itemsData.Add(item.Id);
                var price = BitConverter.GetBytes(item.Price);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(price);
                itemsData.AddRange(price);
                itemsData.Add(item.Max);
                itemsData.Add(item.ReplaceId);
                var replacePrice = BitConverter.GetBytes(item.ReplacePrice);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(replacePrice);
                itemsData.AddRange(replacePrice);
            }
            ++shopId;
        }
        Write((SNES)0xB0C800, [.. shopData]);

        itemsData.AddRange([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]);
        Write((SNES)0xB0C900, [.. itemsData]);
    }

    /// <summary>Set Rupee Arrow mode</summary>
    /// <param name="enable">switch on or off</param>
    public void SetRupeeArrow(bool enable = false)
    {
        Write((SNES)0x068052, [(byte)(enable ? 0xDB : 0xE2)]); // fish bottle merchant
        Write((SNES)0x0681FC, [(byte)(enable ? 0xDA : 0xE1)]); // replace Pot rupees
        Write((SNES)0x1DCB4E, enable ? [0xA9, 0x00, 0xEA, 0xEA] : [0xAF, 0x77, 0xF3, 0x7E]); // thief
        Write((SNES)0x1E8D96, enable ? [0xA9, 0x00, 0xEA, 0xEA] : [0xAF, 0x77, 0xF3, 0x7E]); // pikit
        Write((SNES)0xB08175, [(byte)(enable ? 0x01 : 0x00)]); // enable mode
        Write((SNES)0xB08176, [(byte)(enable ? 0x0A : 0x00), 0x00]); // wood cost
        Write((SNES)0xB08178, [(byte)(enable ? 0x32 : 0x00), 0x00]); // silver cost
        Write((SNES)0x01EDA5, enable ? [0x35, 0x41] : [0x43, 0x44]); // DW chest game
    }

    /// <summary>Set whether Sahasrahla updates your map with Green Pendant when you talk to him</summary>
    /// <param name="reveals">bitfield of what he reveals</param>
    public void SetMapRevealSahasrahla(ushort reveals = 0x0000)
    {
        Span<byte> data = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(data, reveals);
        Write((SNES)0xB0817A, data);
    }

    /// <summary>Set whether Bomb Shop dude updates your map with Red Cyrstals when you talk to him</summary>
    /// <param name="reveals">bitfield of what he reveals</param>
    public void SetMapRevealBombShop(ushort reveals = 0x0000)
    {
        Span<byte> data = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(data, reveals);
        Write((SNES)0xB0817C, data);
    }

    /// <summary>Set it so trade fairies only trade bottles</summary>
    /// <param name="enable">switch on or off</param>
    public void SetRestrictFairyPonds(bool enable = true)
        => Write((SNES)0xB0817E, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable Escape Assist</summary>
    public void SetEscapeAssist(bool infiniteMagic = false, bool infiniteBombs = false, bool infiniteArrows = false)
    {
        byte flags = 0;
        if (infiniteMagic)
            flags |= 0b100;
        if (infiniteBombs)
            flags |= 0b010;
        if (infiniteArrows)
            flags |= 0b001;
        Write((SNES)0xB0804D, [flags]);
    }

    /// <summary>Enable Escape Fills</summary>
    /// <param name="rupees">if rupee bow is enabled, this value is used for starting rupees</param>
    public void SetEscapeFills(bool refillMagic = false, bool refillBombs = false, bool refillArrows = false, ushort rupees = 300)
    {
        byte flags = 0;
        if (refillMagic)
            flags |= 0b100;
        if (refillBombs)
            flags |= 0b010;
        if (refillArrows)
            flags |= 0b001;
        Write((SNES)0xB0804E, [flags]);
        Span<byte> data = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(data, rupees);
        Write((SNES)0xB08183, data);
    }

    /// <summary>Set Uncle Refills on respawn</summary>
    public void SetUncleSpawnRefills(byte magic = 0x00, byte bombs = 0x00, byte arrows = 0x00)
        => Write((SNES)0xB08185, [magic, bombs, arrows]);

    /// <summary>Set Zelda Cell Refills on respawn</summary>
    public void SetZeldaSpawnRefills(byte magic = 0x00, byte bombs = 0x00, byte arrows = 0x00)
        => Write((SNES)0xB08188, [magic, bombs, arrows]);

    /// <summary>Set Mantle Refills on respawn</summary>
    public void SetMantleSpawnRefills(byte magic = 0x00, byte bombs = 0x00, byte arrows = 0x00)
        => Write((SNES)0xB0818B, [magic, bombs, arrows]);

    /// <summary>Set the prizes for the pick 3 chest games.</summary>
    /// <param name="prizes">item id"s of prizes should be length 32</param>
    public void SetChancePrizes(byte[]? prizes = null)
    {
        prizes ??= [
#pragma warning disable format
            // high stakes game
            0x47, 0x34, 0x46, 0x34, 0x46, 0x46, 0x34, 0x47,
            0x46, 0x47, 0x34, 0x46, 0x47, 0x34, 0x46, 0x47,
            // low stakes game
            0x34, 0x47, 0x41, 0x47, 0x41, 0x41, 0x47, 0x34,
            0x41, 0x34, 0x47, 0x41, 0x34, 0x47, 0x41, 0x34,
#pragma warning restore format
        ];

        Write((SNES)0x01EED5, prizes); // 32 bytes
    }

    /// <summary>Set Generic keys mode, if enabled all keys will share 1 pool.</summary>
    /// <param name="enable">switch on or off</param>
    public void SetGenericKeys(bool enable = false)
        => Write((SNES)0xB08172, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Set Smithy Quick Item Give mode. I.E. just gives an item if you rescue him with no sword bogarting</summary>
    /// <param name="enable">switch on or off</param>
    public void SetSmithyQuickItemGive(bool enable = true)
        => Write((SNES)0xB08029, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Set Pyramid Fountain to have 2 chests</summary>
    /// <param name="enable">switch on or off</param>
    public void SetPyramidFairyChests(bool enable = true)
    {
        Write((SNES)0x03FC16, enable
            ? [0xB1, 0xC6, 0xF9, 0xC9, 0xC6, 0xF9]
            : [0xA8, 0xB8, 0x3D, 0xD0, 0xB8, 0x3D]);
    }

    /// <summary>Enable Hammer activates tablets</summary>
    /// <param name="enable">switch on or off</param>
    public void SetHammerTablet(bool enable = false)
        => Write((SNES)0xB08044, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable Hammer breaks Aghanim's barrier no matter what</summary>
    /// <param name="enable">switch on or off</param>
    public void SetHammerBarrier(bool enable = false)
        => Write((SNES)0xB0805D, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable/Disable ability to bug net catch Fairy</summary>
    /// <param name="enable">switch on or off</param>
    public void SetCatchableFairies(bool enable = true)
        => Write((SNES)0x06CFD6, [(byte)(enable ? 0xF0 : 0x80)]);

    /// <summary>Enable which objects stun</summary>
    public void SetStunItems(bool hookshot = true, bool boomerang = true)
    {
        byte flags = 0;
        if (hookshot)
            flags |= 0b10;
        if (boomerang)
            flags |= 0b01;
        Write((SNES)0xB08180, [flags]);
    }

    /// <summary>Enable silver arrows can only be used in Ganon's room</summary>
    /// <param name="enable">switch on or off</param>
    public void SetSilversOnlyAtGanon(bool enable = false)
        => Write((SNES)0xB08181, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Set when silvers equip</summary>
    /// <param name="setting">name</param>
    public void SetSilversEquip(SilversEquipOption setting)
        => Write((SNES)0xB08182, [(byte)setting]);

    /// <summary>Enable/Disable ability to bug net catch Bee (also makes them attack you?)</summary>
    /// <param name="enable">switch on or off</param>
    public void SetCatchableBees(bool enable = true)
    {
        Write((SNES)0x1EDD73, [(byte)(enable ? 0xF0 : 0x80)]);
        Write((SNES)0x1EDF10, [(byte)(enable ? 0xF0 : 0x80)]);
    }

    /// <summary>Place 2 chests in Waterfall of Wishing Fairy.</summary>
    /// <param name="enable">switch on or off</param>
    public void SetWishingWellChests(bool enable = false)
    {
        // set item table to proper room
        Write((SNES)0x01E9AE, enable ? [0x14, 0x01] : [0x05, 0x00]);
        Write((SNES)0x01E9CF, enable ? [0x14, 0x01] : [0x3D, 0x01]);

        // room 276 remodel
        Write((SNES)0x03F714, enable
            ? Convert.FromBase64String(
                "4QAQrA0pmgFYmA8RsWH8TYEg2gIs4WH8voFhsWJU2gL9jYNE4WL9HoMxpckxpGkxwCJNpGkxxvlJxvkQmaBcmaILmGAN6MBV6MALk"
              + "gBzmGD+aQCYo2H+a4H+q4WpyGH+roH/aQLYo2L/a4P/K4fJyGL/LoP+oQCqIWH+poH/IQLKIWL/JoO7I/rDI/q7K/rDK/q7U/rDU/"
              + "qwoD2YE8CYUsCIAGCQAGDoAGDwAGCYysDYysDYE8DYUsD8vYX9HYf/////8P+ALmEOgQ7//w=="
            )
            : Convert.FromBase64String(
                "4QAQrA0pmgFYmA8RsGH8TQEg0gL8vQUs4WH8voFhsGJU0gL9jQP9HQdE4WL9HoMxpckxpGkxwCJNpGkouD1QuD0QmaBcmaILmGAN4"
              + "cBV4cALkgBzmGD+aQCYo2H+a4H+q4WpyGH+roH/aQLYo2L/a4P/K4fJyGL/LoP+oQCqIWH+poH/IQLKIWL/JoO7I/rDI/q7K/rDK/"
              + "q7U/rDU/qwoD2YE8CYUsCIAGCQAGDoAGDwAGCYysDYysDYE8DYUsD/////8P+ALmEOgQ7//w=="
            ));
    }

    /// <summary>Remove 2 statues at hylia fairy</summary>
    /// <param name="enable">switch on or off</param>
    public void SetHyliaFairyShop(bool enable = false)
    {
        Write((SNES)0x03F810, enable
            ? [0x1A, 0x1E, 0x01, 0x1A, 0x1E, 0x01]
            : [0xFC, 0x94, 0xE4, 0xFD, 0x34, 0xE4]);
    }

    /// <summary>Enable/Disable Waterfall of Wishing Fairy's ability to upgrade items.</summary>
    /// <param name="enable">switch on or off</param>
    public void SetWishingWellUpgrade(bool enable = false)
    {
        Write((SNES)0x06C8DB, [(byte)(enable ? 0x0C : 0x2A)]);
        Write((SNES)0x06C8EB, [(byte)(enable ? 0x04 : 0x05)]);
    }

    public void SetGameState(StateOption state)
    {
        SetFixFakeWorld(false);
        switch (state)
        {
            case StateOption.Open:
                //case "retro":
                SetOpenMode(enable: true);
                break;
            case StateOption.Inverted:
                SetInvertedMode(enable: true);
                break;
            case StateOption.Standard:
                SetStandardMode();
                break;
        }
    }

    /// <summary>
    /// Set Game in Open Mode. (Post rain state with Escape undone)
    /// </summary>
    /// <param name="enable">switch on or off</param>
    public void SetOpenMode(bool enable = true)
    {
        SetSewersLampCone(!enable);
        InitialSram.PreOpenCastleGate();
        InitialSram.SetProgressIndicator(0x02);
        InitialSram.SetProgressFlags(0x14);
        InitialSram.SetStartingEntrance(0x01);
    }

    /// <summary>
    /// Set Game in Standard Mode.
    /// </summary>
    public void SetStandardMode()
    {
        SetSewersLampCone(enable: true);
        InitialSram.SetProgressIndicator(0x00);
        InitialSram.SetProgressFlags(0x00);
        InitialSram.SetStartingEntrance(0x00);
    }

    /// <summary>
    /// Set Game in Inverted Mode. (Post rain state with Escape undone and in the Dark Wold with a whole slew of other crap)
    /// </summary>
    /// <param name="enable">switch on or off</param>
    public void SetInvertedMode(bool enable = true)
    {
        // this mode is based on open mode ;)
        SetOpenMode(enable);

        Write((SNES)0xB0804A, [0x01]); // ; main toggle
        Write((SNES)0x0283E0, [0xF0]); // ; residual portal
        Write((SNES)0x02B34D, [0xF0]); // ; residual portal
        Write((SNES)0x06DB78, [0x8B]); // ; residual portal
        Write((SNES)0x05AF79, [0xF0]); // ; vortex
        Write((SNES)0x0DB3C5, [0xC6]); // ; vortex
        Write((SNES)0x07A3F4, [0xF0]); // ; duck
        Write((SNES)0x07A3F4, [0xF0]); // ; duck
        Write((SNES)0x02E849, [0x43, 0x00, 0x56, 0x00, 0x58, 0x00, 0x6c, 0x00, 0x6F, 0x00, 0x70, 0x00, 0x7B, 0x00, 0x7F, 0x00, 0x1B, 0x00]); // ; Dark World Flute Spots
        Write((SNES)0x02E8D5, [0xC8, 0x07]); // ; nudge flute spot 3 out of gargoyle statue
        Write((SNES)0x02E8F7, [0xF8, 0x01]); // ; nudge flute spot 3 out of gargoyle statue
        Write((SNES)0x07A943, [0xF0]); // ; Dark to light world mirror
        Write((SNES)0x07A96D, [0xD0]); // ; residual portal?
        Write((SNES)0x08D40C, [0xD0]); // ; morph poof
        SetFixFakeWorld(enable); // ; ER's Fix fake worlds fix. Currently needed for inverted

        // remove diggable light world portals
        Write((SNES)0x1BC428, [0x00]);
        Write((SNES)0x1BC43A, [0x00]);
        Write((SNES)0x1BC590, [0x00]);
        Write((SNES)0x1BC5A1, [0x00]);
        Write((SNES)0x1BC5B1, [0x00]);
        Write((SNES)0x1BC5C7, [0x00]);

        Write((SNES)0x02DB8C, [0x6C]); // update link's house exit to be dark world (All the other exit table values can be reused)
        Write((SNES)(0x1BBB73 + 0x00), [0x53]); // entering links house door leads to bomb shop
        Write((SNES)(0x1BBB73 + 0x52), [0x01]); // entering bomb shop leads to links house

        // swap GT and AT entrances
        Write((SNES)(0x1BBB73 + 0x23), [0x37]); // entering AT Door Leads to GT
        Write((SNES)(0x1BBB73 + 0x36), [0x24]); // entering GT Door Leads to AT
        Write((SNES)(0x02DAEE + 2 * 0x38), [0xe0, 0x00]); // exiting AT leads to GT
        Write((SNES)(0x02DAEE + 2 * 0x25), [0x0c, 0x00]); // exiting GT leads to AT

        // Bumper Cave (Bottom) => Old Man Cave (West)
        Write((SNES)(0x1BBB73 + 0x15), [0x06]);
        Write((SNES)(0x02DAEE + 2 * 0x17), [0xF0, 0x00]);

        // Old Man Cave (West) => Bumper Cave (Bottom)
        Write((SNES)(0x1BBB73 + 0x05), [0x16]);
        Write((SNES)(0x02DAEE + 2 * 0x07), [0xFB, 0x00]);

        // Death Mountain Return Cave (West) => Bumper Cave (Top)
        Write((SNES)(0x1BBB73 + 0x2D), [0x17]);
        Write((SNES)(0x02DAEE + 2 * 0x2F), [0xEB, 0x00]);

        // Old Man Cave (East) => Death Mountain Return Cave (West)
        Write((SNES)(0x1BBB73 + 0x06), [0x2E]);
        Write((SNES)(0x02DAEE + 2 * 0x08), [0xe6, 0x00]);

        // Bumper Cave (Top) => Dark Death Mountain Fairy
        Write((SNES)(0x1BBB73 + 0x16), [0x5E]);

        // fix trock doors for reverse entrances
        PreOpenBombableWalls();

        // Dark Death Mountain Healer Fairy => Old Man Cave (East)
        Write((SNES)(0x1BBB73 + 0x6F), [0x07]);
        Write((SNES)(0x02DAEE + 2 * 0x18), [0xf1, 0x00]);
        Write((SNES)(0x02DB8C + 0x18), [0x43]);
        Write((SNES)(0x02DBDB + 2 * 0x18), [0x00, 0x14]);
        Write((SNES)(0x02DC79 + 2 * 0x18), [0x94, 0x02]);
        Write((SNES)(0x02DD17 + 2 * 0x18), [0x00, 0x06]);
        Write((SNES)(0x02DDB5 + 2 * 0x18), [0xe8, 0x02]);
        Write((SNES)(0x02DE53 + 2 * 0x18), [0x78, 0x06]);
        Write((SNES)(0x02DEF1 + 2 * 0x18), [0x03, 0x03]);
        Write((SNES)(0x02DF8F + 2 * 0x18), [0x85, 0x06]);
        Write((SNES)(0x02E02D + 0x18), [0x0a]);
        Write((SNES)(0x02E07C + 0x18), [0xf6]);
        Write((SNES)(0x02E0CB + 2 * 0x18), [0x00, 0x00]);
        Write((SNES)(0x02E169 + 2 * 0x18), [0x00, 0x00]);

        // Pyramid Exit <= Houlihan
        Write((SNES)(0x02DAEE + 2 * 0x3D), [0x03, 0x00]);
        Write((SNES)(0x02DB8C + 0x3D), [0x5b]);
        Write((SNES)(0x02DBDB + 2 * 0x3D), [0x0e, 0x0b]);
        Write((SNES)(0x02DC79 + 2 * 0x3D), [0x5a, 0x07]);
        Write((SNES)(0x02DD17 + 2 * 0x3D), [0x74, 0x06]);
        Write((SNES)(0x02DDB5 + 2 * 0x3D), [0xa8, 0x07]);
        Write((SNES)(0x02DE53 + 2 * 0x3D), [0xe8, 0x06]);
        Write((SNES)(0x02DEF1 + 2 * 0x3D), [0xc7, 0x07]);
        Write((SNES)(0x02DF8F + 2 * 0x3D), [0xf3, 0x06]);
        Write((SNES)(0x02E02D + 0x3D), [0x06]);
        Write((SNES)(0x02E07C + 0x3D), [0xfa]);
        Write((SNES)(0x02E0CB + 2 * 0x3D), [0x00, 0x00]);
        Write((SNES)(0x02E169 + 2 * 0x3D), [0x00, 0x00]);

        // Change sanc spawn point to dark sanc
        Write((SNES)0x02D8D4, [0x12, 0x01]);
        Write((SNES)0x02D8E8, [0x22, 0x22, 0x22, 0x23, 0x04, 0x04, 0x04, 0x05]);
        Write((SNES)0x02D91A, [0x00, 0x04]);
        Write((SNES)0x02D928, [0x2e, 0x22]);
        Write((SNES)0x02D936, [0x9a, 0x22]);
        Write((SNES)0x02D944, [0x80, 0x04]);
        Write((SNES)0x02D952, [0xa5, 0x00]);
        Write((SNES)0x02D960, [0x7F, 0x00]);
        Write((SNES)0x02D96D, [0x14]);
        Write((SNES)0x02D974, [0x00]);
        Write((SNES)0x02D97B, [0xFF]);
        Write((SNES)0x02D982, [0x00]);
        Write((SNES)0x02D989, [0x02]);
        Write((SNES)0x02D990, [0x00]);
        Write((SNES)0x02D998, [0x00, 0x00]);
        Write((SNES)0x02D9A6, [0x5A, 0x00]);
        Write((SNES)0x02D9B3, [0x12]);

        // Write dark sanc exit data to StartingAreaExitTable table
        Write((SNES)0xB08250, [
            //0x0112,
            0x12,
            0x01,
            0x53,
            //0x001e, 0x0400, 0x06e2, 0x0446, 0x0758, 0x046d, 0x075f,
            0x1e,
            0x00,
            0x00,
            0x04,
            0xe2,
            0x06,
            0x46,
            0x04,
            0x58,
            0x07,
            0x6d,
            0x04,
            0x5f,
            0x07,
            0x00,
            0x00,
            0x00
        ]);

        // Write to StartingAreaExitOffset table to indicate that dark sanc spawn uses first row in table
        Write((SNES)0xB08240, [0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00]);

        Write((SNES)0xB08350, [0x00, 0x00, 0x01]); // Death mountain cave should start on overworld

        // Change old man spawn point to End of old man cave
        Write((SNES)0x02D8DE, [0xF1, 0x00]);
        Write((SNES)0x02D910, [0x1F, 0x1E, 0x1F, 0x1F, 0x03, 0x02, 0x03, 0x03]);
        Write((SNES)0x02D924, [0x00, 0x03]);
        Write((SNES)0x02D932, [0x10, 0x1F]);
        Write((SNES)0x02D940, [0xC0, 0x1F]);
        Write((SNES)0x02D94E, [0x78, 0x03]);
        Write((SNES)0x02D95C, [0x87, 0x01]);
        Write((SNES)0x02D96A, [0x7F, 0x01]);
        Write((SNES)0x02D972, [0x06]);
        Write((SNES)0x02D979, [0x00]);
        Write((SNES)0x02D980, [0xFF]);
        Write((SNES)0x02D987, [0x00]);
        Write((SNES)0x02D98E, [0x22]);
        Write((SNES)0x02D995, [0x12]);
        Write((SNES)0x02D9A2, [0x00, 0x00]);
        Write((SNES)0x02D9B0, [0x07, 0x00]);
        Write((SNES)0x02D9B8, [0x12]);

        // Write to StartingAreaOverworldDoor table to indicate the overworld door being used for
        // the single entrance spawn point
        Write((SNES)0xB08247, [0x00, 0x5A, 0x00, 0x00, 0x00, 0x00, 0x00]);

        // aga tower exit/ pyramid spawn (now hyrule castle ledge spawn)
        Write((SNES)(0x02DAEE + 2 * 0x06), [0x20, 0x00]);
        Write((SNES)(0x02DB8C + 0x06), [0x1B]);
        Write((SNES)(0x02DBDB + 2 * 0x06), [0xAE, 0x00]);
        Write((SNES)(0x02DC79 + 2 * 0x06), [0x10, 0x06]);
        Write((SNES)(0x02DD17 + 2 * 0x06), [0x7E, 0x07]);
        Write((SNES)(0x02DDB5 + 2 * 0x06), [0x72, 0x06]);
        Write((SNES)(0x02DE53 + 2 * 0x06), [0xF8, 0x07]);
        Write((SNES)(0x02DEF1 + 2 * 0x06), [0x7D, 0x06]);
        Write((SNES)(0x02DF8F + 2 * 0x06), [0x03, 0x08]);
        Write((SNES)(0x02E02D + 0x06), [0x00]);
        Write((SNES)(0x02E07C + 0x06), [0xf2]);
        Write((SNES)(0x02E0CB + 2 * 0x06), [0x00, 0x00]);
        Write((SNES)(0x02E169 + 2 * 0x06), [0x00, 0x00]);

        // move flute spot 9 (notice that the values of this match the 2nd, 3rd etc value of hyrule castle spawn)
        Write((SNES)0x02E87B, [0xae, 0x00]);
        Write((SNES)0x02E89D, [0x10, 0x06]);
        Write((SNES)0x02E8BF, [0x7e, 0x07]);
        Write((SNES)0x02E8E1, [0x72, 0x06]);
        Write((SNES)0x02E903, [0xf8, 0x07]);
        Write((SNES)0x02E925, [0x7d, 0x06]);
        Write((SNES)0x02E947, [0x03, 0x08]);
        Write((SNES)0x02E969, [0x00, 0x00]);
        Write((SNES)0x02E98B, [0xF2, 0xFF]);

        Write((SNES)0x1AF696, [0xF0]); // Bat X position (sprite_retreat_bat.asm:130)
        Write((SNES)0x1AF6B2, [0x33]); // Bat Delay (sprite_retreat_bat.asm:136)

        // New Hole Mask Position
        Write((SNES)0x1AF730, [
            0x6A,
            0x9E,
            0x0C,
            0x00,
            0x7A,
            0x9E,
            0x0C,
            0x00,
            0x8A,
            0x9E,
            0x0C,
            0x00,
            0x6A,
            0xAE,
            0x0C,
            0x00,
            0x7A,
            0xAE,
            0x0C,
            0x00,
            0x8A,
            0xAE,
            0x0C,
            0x00,
            0x67,
            0x97,
            0x0C,
            0x00,
            0x8D,
            0x97,
            0x0C,
            0x00
        ]);

        // redefine some map16 tiles
        Write((SNES)0x0FF1C8, [
            0x0F,
            0x19,
            0x0F,
            0x19,
            0x0F,
            0x19,
            0x4C,
            0x19,
            0x0F,
            0x19,
            0x4B,
            0x19,
            0x0F,
            0x19,
            0x5C,
            0x19,
            0x4B,
            0x59,
            0x4C,
            0x19,
            0xEE,
            0x19,
            0xEE,
            0x19,
            0x4B,
            0x19,
            0xEE,
            0x19,
            0xEE,
            0x19,
            0xEE,
            0x19,
            0x4B,
            0x59,
            0x0F,
            0x19,
            0x5C,
            0x59,
            0x0F,
            0x19,
            0x0F,
            0x19,
            0x5B,
            0x19,
            0x0F,
            0x19,
            0x0F,
            0x19,
            0xEE,
            0x19,
            0xEE,
            0x19,
            0x5C,
            0x19,
            0xEE,
            0x19,
            0xEE,
            0x19,
            0xEE,
            0x19,
            0xEE,
            0x19,
            0x5C,
            0x59,
            0x5B,
            0x59,
            0x0F,
            0x19,
            0x0F,
            0x19,
            0x0F,
            0x19
        ]);

        // Redefine more map16 tiles
        Write((SNES)0x0FA480, [0x0F, 0x19, 0x6B, 0x19, 0x04, 0x9D, 0x04, 0x9D, 0x6B, 0x19, 0x0F, 0x19, 0x04, 0x9D, 0x04, 0x9D]);

        // update pyramid hole entrances
        Write((SNES)0x1bb810, [0xBE, 0x00, 0xC0, 0x00, 0x3E, 0x01]);
        Write((SNES)0x1bb836, [0x1B, 0x00, 0x1B, 0x00, 0x1B, 0x00]);

        // add an extra pyramid hole entrance
        Write((SNES)0xB08300, [0x40, 0x01]); // ExtraHole_Map16
        Write((SNES)0xB08320, [0x1B, 0x00]); // ExtraHole_Area
        Write((SNES)0xB08340, [0x7B]); // ExtraHole_Entrance

        // prioritize retreat Bat and use 3rd sprite group
        Write((SNES)0x1af504, [0x8B, 0x14]);
        Write((SNES)0x1af50c, [0x9B, 0x14]);
        Write((SNES)0x1af514, [0xA4, 0x14]);
        Write((SNES)0x1af51c, [0x89, 0x14]);
        Write((SNES)0x1af524, [0xAC, 0x14]);
        Write((SNES)0x1af52c, [0xAC, 0x54]);
        Write((SNES)0x1af534, [0x8C, 0x14]);
        Write((SNES)0x1af53c, [0x8C, 0x54]);
        Write((SNES)0x1af544, [0x84, 0x14]);
        Write((SNES)0x1af54c, [0x84, 0x54]);
        Write((SNES)0x1af554, [0xA2, 0x14]);
        Write((SNES)0x1af55c, [0xA2, 0x54]);
        Write((SNES)0x1af564, [0xA0, 0x14]);
        Write((SNES)0x1af56c, [0xA0, 0x54]);
        Write((SNES)0x1af574, [0x8E, 0x14]);
        Write((SNES)0x1af57c, [0x8E, 0x54]);
        Write((SNES)0x1af584, [0xAE, 0x14]);
        Write((SNES)0x1af58c, [0xAE, 0x54]);

        // Make retreat bat gfx available in Hyrule castle.
        Write((SNES)0x00DB9D, [0x1A]); // sprite set 1, section 3
        Write((SNES)0x00DC09, [0x1A]); // sprite set 27, section 3

        // use new castle hole graphics (The values are the SNES address of the graphics: 31e000)
        Write((SNES)0x00D009, [0x31]);
        Write((SNES)0x00D0e8, [0xE0]);
        Write((SNES)0x00D1c7, [0x00]);
        Write((SNES)0x1BE8DA, [0xAD, 0x39]); // add color for shading for castle hole

        Write((SNES)0xB08169, [0x02]); // lock aga door
        Write((SNES)0x1EEE58, [0x80]); // don't allow "whirlpool" under castle gate

        // Turtle rock tail
        Write((SNES)0x00886E, [0x5C, 0x00, 0xA0, 0xA1]); // JML.l A1A000 (a.k.a. JML.l InvertedTileAttributeLookup)

        // Add warps under rocks, etc.
        Write((SNES)0x1BC67A, [0x2E, 0x0B, 0x82]); // Replace a rupee under bush to add a warp on map 80 (top of kak)
        Write((SNES)0x1BC81E, [0x94, 0x1D, 0x82]); // Replace a heart under bush to add a warp on map 120 (mire)
        Write((SNES)0x1BC655, [0x4A, 0x1D, 0x82]); // Replace a bomb :( under bush to add a warp on map 78 (DM)
        Write((SNES)0x1BC80D, [0xB2, 0x0B, 0x82]); // map 111
        Write((SNES)0x1BC3DF, [0xD8, 0xD1]); // new pointer for map 115 no items to replace
        Write((SNES)0x1BD1D8, [0xA8, 0x02, 0x82, 0xFF, 0xFF]); // new data for map115
        Write((SNES)0x1BC85A, [0x50, 0x0F, 0x82]);

        // move pyramid exit overworld door
        Write((SNES)(0x1BB96F + 2 * 0x35), [0x1B, 0x00]);
        Write((SNES)(0x1BBA71 + 2 * 0x35), [0xA4, 0x06]);
        Write((SNES)(0x1BBB73 + 0x35), [0x36]);

        // Remove Hyrule Castle Gate warp
        Write((SNES)0x09D436, [0xF3]); // replace whirlpool with (harmless) SpritePositionTarget Overlord

        // Pyramid exits to new hyrule castle area
        Write((SNES)(0x02DAEE + 2 * 0x37), [0x10, 0x00]);
        Write((SNES)(0x02DB8C + 0x37), [0x1B]);
        Write((SNES)(0x02DBDB + 2 * 0x37), [0x18, 0x04]);
        Write((SNES)(0x02DC79 + 2 * 0x37), [0x79, 0x06]);
        Write((SNES)(0x02DD17 + 2 * 0x37), [0xB4, 0x06]);
        Write((SNES)(0x02DDB5 + 2 * 0x37), [0xC6, 0x06]);
        Write((SNES)(0x02DE53 + 2 * 0x37), [0x28, 0x07]);
        Write((SNES)(0x02DEF1 + 2 * 0x37), [0xE6, 0x06]);
        Write((SNES)(0x02DF8F + 2 * 0x37), [0x33, 0x07]);
        Write((SNES)(0x02E02D + 0x37), [0x07]);
        Write((SNES)(0x02E07C + 0x37), [0xf9]);
        Write((SNES)(0x02E0CB + 2 * 0x37), [0x00, 0x00]);
        Write((SNES)(0x02E169 + 2 * 0x37), [0x00, 0x00]);
        Write((SNES)0x1BC387, [0xDD, 0xD1]); // New pointer for map 71 no items to replace
        Write((SNES)0x1BD1DD, [0xA4, 0x06, 0x82, 0x9E, 0x06, 0x82, 0xFF, 0xFF]); // new data for map 71

        Write((SNES)0xB08089, [0x01]); // open TR main entrance on exiting

        Write((SNES)0x0ABFBB, [0x90]); // move mirror portal indicator to correct map (0xB0 normally)

        Write((SNES)0x0280A6, [0xD0]); // Spawn logic

        Write((SNES)0x06B2AB, [0xF0, 0xE1, 0x05]); // frog pickup on contact

        foreach (var (key, text) in YamlReader.LoadDialogForInverted(_text.Language))
            _text.SetString(key, text, !isNoPause(key));

        static bool isNoPause(string key)
        {
            if (Text.IsNoPause(key))
                return true;

            return key is "kiki_leaving_screen"
                       or "dark_sanctuary"
                       or "dark_sanctuary_yes";
        }
    }

    /// <summary>Pre-opens bombable walls that would otherwise trap the player (and be a problem without bombs).</summary>
    public void PreOpenBombableWalls()
    {
        // turtle rock, laser bridge and balcony
        Write((SNES)0x1FED30 + 1, [0x0E]); // RoomDataDoors_0023 +1
        Write((SNES)0x1FEE40 + 1, [0x0E]); // RoomDataDoors_00D5 +1
    }

    /// <summary>Enable maps to show crystals on overworld map</summary>
    /// <param name="requireMap">switch on or off</param>
    public void SetMapMode(bool requireMap = false)
        => Write((SNES)0xB0803B, [(byte)(requireMap ? 0x01 : 0x00)]);

    /// <summary>Enable compass to show dungeon count</summary>
    /// <param name="setting">switch on or off</param>
    public void SetCompassMode(CompassCounterOption setting = CompassCounterOption.Off)
        => Write((SNES)0xB0803C, [(byte)setting]);

    /// <summary>Set Ball and Chain guard dungeon id</summary>
    public void SetBallNChainDungeon(byte dungeonId)
        => Write((SNES)0xB0EFFF, [dungeonId]);

    /// <summary>Set totals for HUD compass counts.</summary>
    public void SetCompassCountTotals(byte[]? totals = null)
    {
        byte[] defaultCounts = [0x08, 0x08, 0x06, 0x06, 0x02, 0x0A, 0x0E, 0x08, 0x08, 0x08, 0x06, 0x08, 0x0C, 0x1B, 0x00, 0x00];
        var compassCounts = totals == null || totals.Length == 0 ? defaultCounts : totals;
        Write((SNES)0xB0F000, compassCounts);
    }

    /// <summary>Enable text box to show with free roaming items</summary>
    public void SetFreeItemTextMode(bool freeCrystals = false, bool outsideDungeonItems = false, bool insideBigKey = false, bool insideMap = false, bool insideCompass = false, bool insideSmallKey = false)
    {
        byte bitField = 0;
        if (freeCrystals)
            bitField |= 0b0010_0000;
        if (outsideDungeonItems)
            bitField |= 0b0001_0000;
        if (insideBigKey)
            bitField |= 0b0000_1000;
        if (insideMap)
            bitField |= 0b0000_0100;
        if (insideCompass)
            bitField |= 0b0000_0010;
        if (insideSmallKey)
            bitField |= 0b0000_0001;

        Write((SNES)0xB0816A, [bitField]);
    }

    /// <summary>Enable free items to show up in menu</summary>
    public void SetFreeItemMenu(bool smallKeys = false, bool bigKey = false, bool map = false, bool compass = false)
    {
        byte flags = 0;
        if (smallKeys)
            flags |= 0b1000;
        if (bigKey)
            flags |= 0b0100;
        if (map)
            flags |= 0b0010;
        if (compass)
            flags |= 0b0001;

        Write((SNES)0xB08045, [flags]);
    }

    /// <summary>Enable swordless mode</summary>
    /// <param name="enable">switch on or off</param>
    public void SetSwordlessMode(bool enable = false)
    {
        Write((SNES)0xB0803F, [(byte)(enable ? 0x01 : 0x00)]); // Hammer Ganon
        Write((SNES)0xB08041, [(byte)(enable ? 0x01 : 0x00)]); // Swordless Medallions
        SetHammerTablet(enable);
        SetHammerBarrier(enable: false);
        if (enable)
            InitialSram.SetSwordlessCurtains();
    }

    /// <summary>Enable lampless light cone in Sewers</summary>
    /// <param name="enable">switch on or off</param>
    public void SetSewersLampCone(bool enable = true)
    {
        byte b = (byte)(enable ? 0x01 : 0x00);
        Write((SNES)0xB08038, [b]);
    }

    /// <summary>Enable/Disable the ROM Hack that doesn't leave Link stranded in DW</summary>
    /// <param name="enable">switch on or off</param>
    public void SetMirrorlessSaveAndQuitToLightWorld(bool enable = true)
    {
        Write((SNES)0xB080A0, [(byte)(enable ? 0x01 : 0x00)]);
    }

    /// <summary>
    /// Sets intro text and, in the future, other game elements to mask settings
    /// visible to the player prior to the start of a run.  This is used when
    /// spoilers is set to "mystery".  This text is currently what is used by
    /// the entrance randomizer.
    /// </summary>
    /// <param name="enable">switch on or off</param>
    public void SetMysteryMasking(bool enable = true)
    {
        if (enable)
        {
            foreach (var (key, text) in YamlReader.LoadDialogForMystery(_text.Language))
                _text.SetString(key, text, !Text.IsNoPause(key));
        }
    }

    /// <summary>Enable/Disable ability to Save and Quit from Boss room after item collection.</summary>
    /// <param name="enable">switch on or off</param>
    public void SetSaveAndQuitFromBossRoom(bool enable = false)
        => Write((SNES)0xB08042, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable/Disable the swamp floodgate state being persistent</summary>
    /// <param name="enable">switch on or off</param>
    public void SetPersistentFloodGate(bool enable = false)
        => Write((SNES)0xB0803D, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable/Disable the ROM Hack that drains the Swamp on transition</summary>
    /// <param name="enable">switch on or off</param>
    public void SetSwampWaterLevel(bool enable = true)
        => Write((SNES)0xB080A1, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable/Disable the ROM Hack that sends Link to Real DW on death in DW dungeon if AG1 is not dead</summary>
    /// <param name="enable">switch on or off</param>
    public void SetPreAgahnimDarkWorldDeathInDungeon(bool enable = true)
        => Write((SNES)0xB080A2, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable/Disable World on Agahnim Death</summary>
    /// <param name="enable">switch on or off</param>
    public void SetWorldOnAgahnimDeath(bool enable = true)
        => Write((SNES)0xB080A3, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable/Disable PoD / S&Q EG correction</summary>
    /// <param name="enable">switch on or off</param>
    public void SetSQEGFix(bool enable = true)
        => Write((SNES)0xB080A4, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable/Disable Allow Accidental Major Glitch</summary>
    /// <param name="enable">switch on or off</param>
    public void SetAllowAccidentalMajorGlitch(bool enable = true)
        => Write((SNES)0xB08358, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Enable/Disable locking Hyrule Castle Door to AG1 during escape</summary>
    /// <param name="enable">switch on or off</param>
    public void SetLockAgahnimDoorInEscape(bool enable = true)
        => Write((SNES)0xB08169, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>
    /// Enable/Disable fix Fake Light World/Fake Dark World as caused by leaving the underworld.
    /// Generally should only be used/enabled by Entrance Randomizer
    /// </summary>
    /// <param name="enable">switch on or off</param>
    public void SetFixFakeWorld(bool enable = false)
        => Write((SNES)0xB08174, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Set the Ganon Warp Phase and Agahnim BB mode</summary>
    /// <param name="setting">name</param>
    public void SetGanonAgahnimRng(GanonAgahnimRngOption setting = GanonAgahnimRngOption.Table)
        => Write((SNES)0xB08086, [(byte)setting]);

    /// <summary>Set the Tower Crystal Requirement</summary>
    public void SetTowerCrystalRequirement(int crystals = 7)
        => Write((SNES)0xB0819A, [(byte)Math.Max(Math.Min(crystals, 7), 0)]);

    /// <summary>Set the Ganon Crystal Requirement</summary>
    public void SetGanonCrystalRequirement(int crystals = 7)
        => Write((SNES)0xB081A6, [(byte)Math.Max(Math.Min(crystals, 7), 0)]);

    /// <summary>Set the number of Moldorm's eyes (minimum 0, maximum 8 eyes) and make it derpy by increasing the gap</summary>
    public void SetMoldormEyeCount(int moldormEyeCount = 2, int moldormDerpAmount = 2)
    {
        if (moldormEyeCount == 0)
        {
            // special case: no eyes means we can just skip the drawing routine.
            Write((SNES)0x1DD889, [NOP, NOP, NOP]); // JSR SpriteDraw_Moldorm_Eyeballs
        }
        else
        {
            // the eye count is a loop variable, which goes from [0..8)
            // it always runs at least once; subtract one to get the right number of eyes.
            int eyeCount = Math.Clamp(moldormEyeCount - 1, 0, 7);
            Write((SNES)0x1DDBB3, [(byte)eyeCount]);
            // eye distance, which makes Moldorm more derpy.
            // 0 is boring (same as 1 eye), anything beyond 8 just wraps back around.
            int derpAmount = Math.Clamp(moldormDerpAmount, 1, 8);
            Write((SNES)0x1DDC06, [(byte)derpAmount]);
        }
    }
    /// <summary>Set tile rooms to use the specified pattern.</summary>
    public void SetTileRoomPattern(TileRoomPattern pattern)
    {
        if (pattern is null)
            return;
        if (pattern.Tiles.Length > 0x16)
            throw new ArgumentException($"Tile Pattern has {pattern.Tiles.Length} tiles, max. supported is 0x16.", nameof(pattern));

        byte speed = pattern.Speed;
        if (speed == 0)
            speed = 0xE0;

        // Overlord14_TileRoom.continue, LDA.b #$E0
        Write((SNES)0x09BA21, [speed]);
        // Overlord14_TileRoom.continue, CMP.b #$16
        Write((SNES)0x09BA1D, [(byte)pattern.Tiles.Length]);

        // SpawnFlyingTile.position_x/.position_y
        for (int i = 0; i < pattern.Tiles.Length; i++)
        {
            Write((SNES)0x09BA2A + i, [(byte)((pattern.Tiles[i].X + 3) * 16)]);
            Write((SNES)0x09BA2A + 0x16 + i, [(byte)((pattern.Tiles[i].Y + 4) * 16)]);
        }
    }

    /// <summary>Set starting with Pseudo Boots.</summary>
    public void SetPseudoBoots(bool enable = false)
        => Write((SNES)0xB0808E, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Write the seed identifier</summary>
    /// <param name="seed">identifier for this seed</param>
    public void SetSeedString(string seed)
        => Write((SNES)0x00FFC0, [.. seed.MaxLength(21).Select(c => (byte)c)]);

    /// <summary>Write a block of data to RNG Block in ROM.</summary>
    /// <param name="random">prng byte generator</param>
    public void WriteRNGBlock(Func<byte> random)
    {
        var rng = new List<byte>();
        for (int i = 0; i < 1024; i++)
            rng.Add(random());
        Write((SNES)0xAF8000, [.. rng]);
    }

    /// <summary>set the flags byte in ROM</summary>
    /// <remarks>
    /// dgGe mutT (bitmask)
    /// d - Nonstandard Dungeon Configuration (Not Map/Compass/BigKey/SmallKeys in same quantity as vanilla)
    /// g - Requires Minor Glitches (Fake flippers, bomb jumps, etc)
    /// G - Requires Major Glitches (OW YBA/Clips, etc)
    /// e - Requires EG
    /// m - Contains Multiples of Major Items
    /// u - Contains Unreachable Items
    /// t - Minor Trolling (Swapped around levers, etc)
    /// T - Major Trolling (Forced-guess softlocks, impossible seed, etc)
    /// </remarks>
    public void SetWarningFlags(
        bool nonStandardDungeons = false,
        bool requiresMinorGlitches = false,
        bool requiresMajorGlitches = false,
        bool requiresExplorationGlitch = false,
        bool containsMultiplesOfMajorItems = false,
        bool containsUnreachableItems = false,
        bool minorTrolling = false,
        bool majorTrolling = false)
    {
        byte flags = 0;
        if (nonStandardDungeons)
            flags |= 0b1000_0000;
        if (requiresMinorGlitches)
            flags |= 0b0100_0000;
        if (requiresMajorGlitches)
            flags |= 0b0010_0000;
        if (requiresExplorationGlitch)
            flags |= 0b0001_0000;
        if (containsMultiplesOfMajorItems)
            flags |= 0b0000_1000;
        if (containsUnreachableItems)
            flags |= 0b0000_0100;
        if (minorTrolling)
            flags |= 0b0000_0010;
        if (majorTrolling)
            flags |= 0b0000_0001;

        Write((SNES)0xB08212, [flags]);
    }

    /// <summary>Mute all audio tracks.</summary>
    /// <param name="enable">switch on or off</param>
    public void MuteMusic(bool enable = true)
        => Write((SNES)0xB0821A, [(byte)(enable ? 0x01 : 0x00)]);

    /// <summary>Write the initial save data table.</summary>
    public void WriteInitialSram()
        => Write((SNES)0xB0B000, InitialSram.GetInitialSram());

    /// <summary>
    /// Write the total number of collectable items in the game. This applies to
    /// items with the "item get" animation but not dungeon prizes, absorbable keys,
    /// or shop items.
    /// </summary>
    /// <param name="count">total number of items</param>
    public void SetTotalItemCount(ushort count)
    {
        Span<byte> data = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(data, count);
        Write((SNES)0xB08196, data);
    }

    /// <summary>Set Zelda Save and Quit Mirror Fix</summary>
    public void SetZeldaMirrorFix(bool enable = true)
        => Write((SNES)0x02D9A8, [(byte)(enable ? 0x04 : 0x02)]);

    /// <summary>Set CPU speed written to MEMSEL on boot.</summary>
    public void EnableFastRom(bool enable = true)
        => Write((SNES)0xB0F032, [(byte)(enable ? 0x01 : 0x00)]);

    public void WriteSprite(Vertex location, Sprite? spriteToWrite = null)
    {
        if (location?.Addresses == null)
            return;

        spriteToWrite ??= location.Sprite;
        if (spriteToWrite == null)
            return;

        var spriteByte = spriteToWrite.Id;
        long address = location.Addresses[0];
        Write((SNES)address, [spriteByte]);
    }

    public void WriteItem(Vertex location, Item? itemToWrite = null)
    {
        if (location?.Addresses == null)
            return;

        itemToWrite ??= location.Item as Item;
        if (itemToWrite == null)
            return;

        var itemBytes = itemToWrite.Bytes;
        // probably a meta item...
        // FIXME: or something that needs special handling?
        if (itemBytes == null)
            return;

        for (int i = 0; i < Math.Min(itemBytes.Length, location.Addresses.Length); i++)
        {
            if (i >= location.Addresses.Length)
                break;
            long address = location.Addresses[i];
            byte? itemByte = itemBytes.ElementAtOrDefault(i);
            if (itemByte == null)
                continue;

            Write((SNES)address, [itemByte.Value]);
        }
    }
    public void WriteCreditsText(WorldConfig config, Vertex location, Item? item)
    {
        var (creditsKey, creditsTextMap) = location.Name switch
        {
            "Master Sword Pedestal" => ("pedestal", YamlReader.LoadCreditsForPedestal(config.Language)),
            "Link's Uncle" => ("house", YamlReader.LoadCreditsForUncle(config.Language)),
            "King Zora" => ("zora", YamlReader.LoadCreditsForZora(config.Language)),
            "Potion Shop Item" => ("witch", YamlReader.LoadCreditsForWitchHut(config.Language)),
            "Sick Kid Item" => ("kakariko2", YamlReader.LoadCreditsForSickKid(config.Language)),
            "Flute Spot" => ("grove", YamlReader.LoadCreditsForFluteSpot(config.Language)),
            _ => (null, null),
        };

        if (string.IsNullOrEmpty(creditsKey))
            return;

        string creditsText = "simply nothing";
        if (creditsTextMap != null && item != null)
        {
            if (creditsTextMap.TryGetValue(item.Name, out var specificItemText))
                creditsText = specificItemText;
            else if (creditsTextMap.TryGetValue("default", out var fallbackText))
                creditsText = fallbackText;
        }
        SetCredit(creditsKey, creditsText);
    }

    private static readonly byte[] _musicChoices =
    [
        0x11, // pendant
        0x16, // crystal
    ];
    public void WriteDungeonMusic(Vertex location, Item item, PRNG prng)
    {
        if (location?.SubType != VertexType.Prize)
            return;
        var musicAddresses = location.GetDungeonMusicAddresses();
        if (musicAddresses == null)
            return;

        byte music;
        var config = ((World)location.World).Config;
        if (config.RegionWildMaps)
            music = prng.GetRandomElement(_musicChoices);
        else
            music = item.Name.StartsWith("Crystal") ? (byte)0x16 : (byte)0x11;

        foreach (int address in musicAddresses)
            Write((SNES)address, [music]);
    }

    public void WriteHintText(WorldConfig config, Vertex location, Item? item)
    {
        var (hintKey, hintTextMap) = location.Name switch
        {
            "Master Sword Pedestal" => ("mastersword_pedestal_translated", YamlReader.LoadHintsForPedestal(config.Language)),
            "Ether Tablet" => ("tablet_ether_book", YamlReader.LoadHintsForEtherTablet(config.Language)),
            "Bombos Tablet" => ("tablet_bombos_book", YamlReader.LoadHintsForBombosTablet(config.Language)),
            _ => (null, null),
        };

        if (string.IsNullOrEmpty(hintKey))
            return;

        string hintText = "Don't waste\nyour time!";
        if (hintTextMap != null && item != null)
        {
            if (hintTextMap.TryGetValue(item.Name, out var specificItemText))
                hintText = specificItemText;
            else if (hintTextMap.TryGetValue("default", out var fallbackText))
                hintText = fallbackText;
        }
        SetText(hintKey, hintText);
    }

    public void WriteLocationSpecificData(Vertex location, Item? item)
    {
        if (item == null)
            return;

        switch (location?.Name)
        {
            case "Tower Of Hera - Basement Cage":
                // in case this location is a hera key, be vanilla and don't allow players to
                // pick up the key with a boomerang. any other item is fair game though.
                if (item?.Name != "KeyP3" && location.World.GetLocation("Tower Of Hera - Basement Cage - Key") is Vertex basementCageKey)
                {
                    // this isn't _really_ a heart piece, but what we'd usually patch over the tile sprite (for boomerang pickups).
                    basementCageKey.Sprite = Sprite.Get("HeartPiece");
                }
                break;
        }
    }

    /// <summary>Write outlets, entrances and holes to the rom.</summary>
    /// <param name="outlets">RoomId to OutletId map</param>
    /// <param name="entrances">EntranceId to InletId map</param>
    /// <param name="holes">EntranceId to InletId map</param>
    public void WriteEntrances(IDictionary<int, int> outlets, IDictionary<int, int> entrances, IDictionary<int, int> holes)
    {
        // RoomToOutlet (tables.asm), offset is the room id pointing towards the related outlet id
        foreach (var (roomId, outletId) in outlets)
            Write((SNES)(0x30EB00 + roomId), [(byte)outletId]);
        // Overworld_Entrance_ID (Vanilla, bank_1B.asm)
        foreach (var (entranceId, inletId) in entrances)
            Write((SNES)(0x1BBB73 + entranceId), [(byte)inletId]);
        // Overworld_GetPitDestination_entrance (Vanilla, bank_1B.asm)
        foreach (var (entranceId, inletId) in holes)
            Write((SNES)(0x1BB84C + entranceId), [(byte)inletId]);
    }

    /// <summary>
    /// Reads the enemy damage table from the ROM and returns it.
    /// </summary>
    public byte[] GetEnemyDamageTable()
        => _gameData.Enemy.Damage.ToArray();

    /// <summary>
    /// Writes the enemy damage table to the ROM.
    /// </summary>
    /// <param name="damageTable">The damage table to write.</param>
    public void SetEnemyDamageTable(byte[] damageTable, PRNG prng)
    {
        // Vanilla "bump" damage table (SpriteData_Bump, bank_0D.asm)
        Write((SNES)0x0DB266, damageTable);

        // SpritePrep_Rat_damage (sprite 0x6D)
        Write((SNES)0x068874, [(byte)prng.GetRandomInt(0..9), (byte)prng.GetRandomInt(0..9)]);
        // SpritePrep_Keese_damage (sprite 0x6F)
        Write((SNES)0x068888, [(byte)prng.GetRandomInt(0..9), (byte)prng.GetRandomInt(0..9)]);
        // SpritePrep_Rope_damage (sprite 0x6E)
        Write((SNES)0x0688A4, [(byte)prng.GetRandomInt(0..9), (byte)prng.GetRandomInt(0..9)]);
        // SpritePrep_Raven_damage (sprite 0x00)
        Write((SNES)0x068963, [(byte)prng.GetRandomInt(0..9), (byte)prng.GetRandomInt(0..9)]);
        // SpritePrep_Tektite_damage (sprite 0xC9)
        Write((SNES)0x068D99, [(byte)prng.GetRandomInt(0..9), (byte)prng.GetRandomInt(0..9)]);
        // SpritePrep_Octorok_damage (sprites 0x08/0x0A)
        Write((SNES)0x068F74, [(byte)prng.GetRandomInt(0..9), (byte)prng.GetRandomInt(0..9)]);
        // SpritePrep_HardhatBeetle_bump (sprite 0x26)
        Write((SNES)0x069127, [(byte)prng.GetRandomInt(0..9), (byte)prng.GetRandomInt(0..9)]);
        // patch the damage value for powdered blobs (sprite 0x8D)
        Write((SNES)0x06EE0B, [(byte)prng.GetRandomInt(0..9)]);
    }

    /// <summary>
    /// Reads the enemy health table from the ROM and returns it.
    /// </summary>
    public byte[] GetEnemyHealthTable()
        => _gameData.Enemy.Health.ToArray();

    /// <summary>
    /// Writes the enemy health table to the ROM.
    /// </summary>
    /// <param name="healthTable">The health table to write.</param>
    /// <param name="lowest">The lowest of the range of health values to use.</param>
    /// <param name="highest">The highest of the range of health values to use.</param>
    /// <param name="prng">The PRNG to use for randomization.</param>
    public void SetEnemyHealthTable(byte[] healthTable, int lowest, int highest, PRNG prng)
    {
        // Vanilla health table (SpriteData_Health, bank_0D.asm)
        Write((SNES)0x0DB173, healthTable);

        // Health values for sprites that appear in both light/dark world
        // SpritePrep_Rat_hp (sprite 0x6D)
        Write((SNES)0x068876, [(byte)prng.GetRandomInt(lowest..highest), (byte)prng.GetRandomInt(lowest..highest)]);
        // SpritePrep_Keese_hp (sprite 0x6F)
        Write((SNES)0x06888A, [(byte)prng.GetRandomInt(lowest..highest), (byte)prng.GetRandomInt(lowest..highest)]);
        // SpritePrep_Rope_hp (sprite 0x6E)
        Write((SNES)0x0688A6, [(byte)prng.GetRandomInt(lowest..highest), (byte)prng.GetRandomInt(lowest..highest)]);
        // SpritePrep_Raven_hp (sprite 0x00)
        Write((SNES)0x068965, [(byte)prng.GetRandomInt(lowest..highest), (byte)prng.GetRandomInt(lowest..highest)]);
        // SpritePrep_Tektite_health (sprite 0xC9)
        Write((SNES)0x068D97, [(byte)prng.GetRandomInt(lowest..highest), (byte)prng.GetRandomInt(lowest..highest)]);
        // SpritePrep_Octorok_health (sprites 0x08/0x0A)
        Write((SNES)0x068F76, [(byte)prng.GetRandomInt(lowest..highest), (byte)prng.GetRandomInt(lowest..highest)]);
        // SpritePrep_HardhatBeetle_health (sprite 0x26)
        Write((SNES)0x06911F, [(byte)prng.GetRandomInt(lowest..highest), (byte)prng.GetRandomInt(lowest..highest)]);
    }

    public void WriteUnderworldEnemies(byte[] table, ushort[] offsets, byte[] spriteSheets, RoomHeaderPatches roomHeaderChanges)
    {
        // full room headers (roomheaders.asm)
        // 32 bytes per entry, offset 0x10 for the 4 sprite sheet ids
        // offset 3 (the old sprite sheet set id) is unused
        for (int i = 0; i < spriteSheets.Length / 4; i++)
        {
            Write((SNES)(0xB58000 + (i * 32) + 0x10), [spriteSheets[(i * 4) + 0], spriteSheets[(i * 4) + 1], spriteSheets[(i * 4) + 2], spriteSheets[(i * 4) + 3]]);
            if (roomHeaderChanges.TryGet(i, out var roomHeaderPatch))
            {
                if (roomHeaderPatch.Background2Properties.HasValue)
                    Write((SNES)(0xB58000 + (i * 32) + 0x0), [roomHeaderPatch.Background2Properties.Value]);
                if (roomHeaderPatch.BlockSet.HasValue)
                    Write((SNES)(0xB58000 + (i * 32) + 0x2), [roomHeaderPatch.BlockSet.Value]);
                if (roomHeaderPatch.BackgroundMove.HasValue)
                    Write((SNES)(0xB58000 + (i * 32) + 0x4), [roomHeaderPatch.BackgroundMove.Value]);
                if (roomHeaderPatch.Effect1.HasValue)
                    Write((SNES)(0xB58000 + (i * 32) + 0x5), [roomHeaderPatch.Effect1.Value]);
                if (roomHeaderPatch.Effect2.HasValue)
                    Write((SNES)(0xB58000 + (i * 32) + 0x6), [roomHeaderPatch.Effect2.Value]);
            }
        }

        // SNES table start _09D62E (RoomData_SpritePointers)
        int dataStart = 0x09D62E + offsets.Length * 2;
        Span<byte> data = stackalloc byte[2];
        foreach (var (roomId, offset) in offsets.Indexed())
        {
            BinaryPrimitives.WriteUInt16LittleEndian(data, (ushort)(dataStart + offset));
            Write((SNES)(0x09D62E + roomId * 2), data);
        }
        Write((SNES)(0x09D62E + offsets.Length * 2), table);
    }
    public void WriteOverworldEnemies(byte[] table, ushort[] offsets, List<ushort>[] statePointerOffsets, byte[] spriteSheets, byte[] specialSpriteSheets)
    {
        if (table.Length > 0x0B29)
            throw new Exception("Trying to write too many enemy sprites to OW!");
        if (specialSpriteSheets.Length > 0x04 * 4)
            throw new Exception($"Trying to write too many special world sprite sheets (got 0x{specialSpriteSheets.Length:X02} which exceeds 0x10)");

        // Pointer to pointer table
        Span<byte> data = stackalloc byte[2];
        int statePointerStart = 0x09C881; // Overworld_SpritePointers
        // patch LDA.w operand to the correct offsets
        foreach (var (state, pointerOffsets) in statePointerOffsets.Indexed())
        {
            var statePointerLDA = state switch
            {
                0 => 0x09C4EF, // Overworld_LoadSprites, rain state
                1 => 0x09C503, // Overworld_LoadSprites.zelda_rescued
                2 => 0x09C4F9, // Overworld_LoadSprites.aga_dead
                _ => throw new ArgumentOutOfRangeException($"Expected a light world state (0, 1 or 2), got {state} instead.")
            };

            foreach (ushort offset in pointerOffsets)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(data, (ushort)(statePointerStart + offset));
                Write((SNES)(statePointerLDA + 1), data);

                BinaryPrimitives.WriteUInt16LittleEndian(data, (ushort)(statePointerStart + offset + 1));
                Write((SNES)(statePointerLDA + 6), data);
            }
        }

        int dataStart = statePointerStart + offsets.Length * 2;
        foreach (var (map, offset) in offsets.Indexed())
        {
            BinaryPrimitives.WriteUInt16LittleEndian(data, (ushort)(dataStart + offset));
            Write((SNES)(statePointerStart + map * 2), data);
        }
        Write((SNES)dataStart, table);

        // OW sheets 0x00FA41 (Sprite_LoadGraphicsProperties)
        Write((SNES)0x00FA41, spriteSheets);
        // special OW 0x02E575 // zora/msp/hobo
        Write((SNES)0x02E575, specialSpriteSheets);
    }

    public void WriteSpriteSheetSets(byte[] spriteSheetSets)
    {
        if (spriteSheetSets.Length > 0xBF * 4)
            throw new Exception($"Trying to write too many sprite sheet sets (got 0x{spriteSheetSets.Length / 4:X02} which exceeds 0xBF)");

        Write((SNES)0x00DB97, spriteSheetSets);
    }

    public void WriteUnderworldRoomsChanges(RoomObjectPatches roomPatches)
    {
        // RoomData_ObjectDataPointers
        var roomDataTiles = (SNES)0x1F8000;
        // RoomData_DoorDataPointers
        var roomDataDoors = (SNES)0x1F83C0;
        // space used by door rando to store modified rooms that don't fit anywhere else (0x8000)
        var freeRoomSpaceBegin = (SNES)0x378000;
        var freeRoomSpace = freeRoomSpaceBegin;

        Span<byte> data = stackalloc byte[4];
        var unusedData = new List<(int Start, int Length)>();
        var newData = new List<(int RoomId, byte[] Data, int DoorStart)>();

        foreach (var roomId in roomPatches.Rooms)
        {
            var roomDataHeader = _gameData.Rooms.FirstOrDefault(r => r.Room == roomId);
            if (roomDataHeader == null)
            {
                throw new ArgumentOutOfRangeException($"Room ID {roomId} does not exist in the game data.");
            }

            var upperLayer = roomDataHeader.UpperLayer; // usually floor/wall data
            var lowerLayer = roomDataHeader.LowerLayer; // usually background under the floor
            var priorityLayer = roomDataHeader.PriorityLayer; // upper priority layer, overwrites the upper layer if something draws over both upper/lower
            var doorData = roomDataHeader.DoorData;
            var oldDataLength = 2 + upperLayer.Length + 2 + lowerLayer.Length + 2 + priorityLayer.Length + 2 + doorData.Length + 2;
            int roomDataStart = roomDataHeader.TilesPtr;

            byte layout = roomDataHeader.Layout; // wall layout of the room quad
            byte floor1 = roomDataHeader.Floor1; // upper/lower floor tile pattern
            byte floor2 = roomDataHeader.Floor2;
            if (roomPatches.TryGet(roomId, out var roomPatch))
            {
                if (roomPatch.Layout is { } layoutOverride)
                    layout = layoutOverride;
                if (roomPatch.Floor1 is { } floor1Override)
                    floor1 = floor1Override;
                if (roomPatch.Floor2 is { } floor2Override)
                    floor2 = floor2Override;
                if (roomPatch.UpperLayer is { } upperLayerOverride)
                    upperLayer = upperLayerOverride;
                if (roomPatch.LowerLayer is { } lowerLayerOverride)
                    lowerLayer = lowerLayerOverride;
                if (roomPatch.PriorityLayer is { } priorityLayerOverride)
                    priorityLayer = priorityLayerOverride;
            }

            var doorStartRel = roomDataHeader.DoorPtr - roomDataHeader.TilesPtr;
            byte[] newRoomData = [(byte)((floor2 << 4) | floor1), (byte)(layout << 2), .. upperLayer, 0xFF, 0xFF, .. lowerLayer, 0xFF, 0xFF, .. priorityLayer, 0xF0, 0xFF, .. doorData, 0xFF, 0xFF];
            int newDoorStartRel = newRoomData.Length - doorData.Length - 2;

            if (newRoomData.Length <= oldDataLength)
            {
                _logger.LogDebug("Updated Room 0x{RoomId:X02} in-place at 0x{RoomDataAddress:X06} (0x{NewSize:X04} <= 0x{AvailableSize:X04} bytes)",
                    roomId, roomDataStart, newRoomData.Length, oldDataLength);
                // we got enough space; write back to the old location
                Write((SNES)roomDataStart, newRoomData);
                // patch the door data start; it is right after the room data
                BinaryPrimitives.WriteUInt32LittleEndian(data, (uint)ToFastRom(roomDataStart + newDoorStartRel));
                Write(roomDataDoors + (3 * roomId), data[..3]);
            }
            else
            {
                _logger.LogDebug("Queuing Room 0x{RoomId:X02} for relocation, doesn't fit 0x{RoomDataAddress:X06} (0x{NewSize:X04} > 0x{AvailableSize:X04} bytes)",
                    roomId, roomDataStart, newRoomData.Length, oldDataLength);
                // we need more space now (additional object data), queue up for later
                unusedData.Add((roomDataStart, oldDataLength));
                newData.Add((roomId, newRoomData, newDoorStartRel));
            }
        }

        foreach (var (roomId, newRoomData, doorStart) in newData.OrderByDescending(d => d.Data.Length))
        {
            var unusedSpot = unusedData.Where(d => d.Length >= newRoomData.Length).OrderBy(d => d.Length).FirstOrDefault();
            int roomDataStart;
            if (unusedSpot.Length >= newRoomData.Length)
            {
                // we moved a larger room elsewhere; reuse the space
                roomDataStart = unusedSpot.Start;
                unusedData.Remove(unusedSpot);
            }
            else
            {
                roomDataStart = freeRoomSpace.Value;
                freeRoomSpace += newRoomData.Length;
            }

            _logger.LogDebug("Relocated Room 0x{RoomId:X02} to 0x{RoomDataAddress:X06} (0x{NewSize:X04} <= 0x{AvailableSize:X04} bytes)",
                roomId, roomDataStart, newRoomData.Length,
                unusedSpot.Length > 0 ? unusedSpot.Length : (0x8000 - (freeRoomSpaceBegin.Value - freeRoomSpace.Value + newRoomData.Length)));
            Write((SNES)roomDataStart, newRoomData);
            // patch the room/tile data start
            BinaryPrimitives.WriteUInt32LittleEndian(data, (uint)ToFastRom(roomDataStart));
            Write(roomDataTiles + (3 * roomId), data[..3]);
            // patch the door data start; it is right after the room data
            BinaryPrimitives.WriteUInt32LittleEndian(data, (uint)ToFastRom(roomDataStart + doorStart));
            Write(roomDataDoors + (3 * roomId), data[..3]);
        }
    }
    private static int FromFastRom(int fastRomAddress) => fastRomAddress & 0x007F_FFFF;
    private static int ToFastRom(int slowRomAddress) => slowRomAddress | 0x0080_0000;
}
