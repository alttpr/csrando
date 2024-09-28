namespace Randomizer.Games.Alttp;

using System.Numerics;
using Randomizer.Graph;

/// <summary>
/// Wrapper class around an array holding an initial SRAM table which
/// is written to the ROM and copied on new save file creation + The class has
/// methods to alter the initial SRAM state and a <see cref="GetInitialSram"/> method which
/// produces an array of ints with values 0-255 and a length the size of <see cref="SRAM_SIZE"/>.
/// </summary>
internal sealed class InitialSram
{
    private const int SRAM_SIZE = 0x500;
    private const int ROOM_DATA = 0x000;
    private const int OVERWORLD_DATA = 0x280;
    private readonly byte[] _initialSramBytes = new byte[SRAM_SIZE];

    /// <summary>
    /// Constructor that fills the array with zeroes, pre-opens Kakariko bomb
    /// hut and brewery, and sets default ability flags.
    /// </summary>
    public InitialSram()
    {
        Array.Fill(_initialSramBytes, (byte)0);
        _initialSramBytes[ROOM_DATA + 0x20D] = 0xF0;
        _initialSramBytes[ROOM_DATA + 0x20F] = 0xF0;
        _initialSramBytes[0x379] = 0b01101000;
        _initialSramBytes[0x401] = 0xFF;
        _initialSramBytes[0x402] = 0xFF;
    }

    /// <summary>
    /// Sets an index to a value in the initial SRAM table using a bitwise OR
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">if the index is greater than <see cref="SRAM_SIZE"/></exception>
    private void SetValue(int idx, byte val)
    {
        if (idx is < 0 or > SRAM_SIZE)
            throw new ArgumentOutOfRangeException(nameof(idx), idx, "Initial SRAM index out of bounds: " + idx);

        _initialSramBytes[idx] |= val;
    }

    /// <summary>Gets a value</summary>
    private byte GetValue(int idx) => _initialSramBytes[idx];

    // Room data

    /// <summary>Pre-opens Aga Tower curtains</summary>
    public void PreOpenAgaCurtains() => SetValue(ROOM_DATA + 0x61, 0x80);

    /// <summary>Pre-opens Skull Woods curtains</summary>
    public void PreOpenSkullWoodsCurtains() => SetValue(ROOM_DATA + 0x93, 0x80);

    // Overworld data

    /// <summary>Pre-opens Hyrule Castle Gate</summary>
    public void PreOpenCastleGate() => SetValue(OVERWORLD_DATA + 0x1B, 0x20);

    /// <summary>Pre-opens Ganon's Tower</summary>
    public void PreOpenGanonsTower() => SetValue(OVERWORLD_DATA + 0x43, 0x20);

    /// <summary>Pre-opens pyramid hole</summary>
    public void PreOpenPyramid() => SetValue(OVERWORLD_DATA + 0x5B, 0x20);

    /// <summary>set the items passed in as Link's starting equipment</summary>
    /// <param name="items">items to equip Link with</param>
    public void SetStartingEquipment(Inventory items, World world)
    {
        var config = world.Config;
        int startingRupees = 0;
        byte startingArrowCapacity = 0;
        byte startingBombCapacity = 0;
        // starting heart containers
        if (items.HeartCount(world) < 1)
        {
            _initialSramBytes[0x36C] = 0x18;
            _initialSramBytes[0x36D] = 0x18;
        }

        foreach (var (item, count) in items.All())
        {
            switch (item.Name)
            {
                case "L1Sword":
                    _initialSramBytes[0x359] = 0x01;
                    _initialSramBytes[0x417] = 0x01;
                    break;
                case "L1SwordAndShield":
                    _initialSramBytes[0x359] = 0x01;
                    _initialSramBytes[0x35A] = 0x01;
                    _initialSramBytes[0x417] = 0x01;
                    _initialSramBytes[0x422] = 0x01;
                    break;
                case "L2Sword":
                case "MasterSword":
                    _initialSramBytes[0x359] = 0x02;
                    _initialSramBytes[0x417] = 0x02;
                    break;
                case "L3Sword":
                    _initialSramBytes[0x359] = 0x03;
                    _initialSramBytes[0x417] = 0x03;
                    break;
                case "L4Sword":
                    _initialSramBytes[0x359] = 0x04;
                    _initialSramBytes[0x417] = 0x04;
                    break;
                case "BlueShield":
                    _initialSramBytes[0x35A] = 0x01;
                    _initialSramBytes[0x422] = 0x01;
                    break;
                case "RedShield":
                    _initialSramBytes[0x35A] = 0x02;
                    _initialSramBytes[0x422] = 0x02;
                    break;
                case "MirrorShield":
                    _initialSramBytes[0x35A] = 0x03;
                    _initialSramBytes[0x422] = 0x03;
                    break;
                case "FireRod":
                    _initialSramBytes[0x345] = 0x01;
                    break;
                case "IceRod":
                    _initialSramBytes[0x346] = 0x01;
                    break;
                case "Hammer":
                    _initialSramBytes[0x34B] = 0x01;
                    break;
                case "Hookshot":
                    _initialSramBytes[0x342] = 0x01;
                    break;
                case "Bow":
                    _initialSramBytes[0x340] = 0x01;
                    if (!config.RomRupeeBow)
                        _initialSramBytes[0x38E] |= 0b1000_0000;
                    break;
                case "BowAndArrows":
                    _initialSramBytes[0x340] = 0x02;
                    _initialSramBytes[0x38E] |= 0b1000_0000;
                    if (config.RomRupeeBow)
                        _initialSramBytes[0x377] = 0x01;
                    break;
                case "SilverArrowUpgrade":
                    _initialSramBytes[0x38E] |= 0b0100_0000;
                    if (config.RomRupeeBow)
                        _initialSramBytes[0x377] = 0x01;
                    break;
                case "BowAndSilverArrows":
                    _initialSramBytes[0x340] = 0x04;
                    _initialSramBytes[0x38E] |= 0b0100_0000;
                    if (config.RomRupeeBow)
                        _initialSramBytes[0x377] = 0x01;
                    else
                        _initialSramBytes[0x38E] |= 0b1000_0000;
                    break;
                case "ProgressiveBow":
                    _initialSramBytes[0x340] = (byte)Math.Min(_initialSramBytes[0x340] + 2 * count, 4);
                    if (config.RomRupeeBow)
                        _initialSramBytes[0x377] = 0x01;
                    else
                        _initialSramBytes[0x38E] = 0b1000_0000;
                    break;
                case "Boomerang":
                    _initialSramBytes[0x341] = 0x01;
                    _initialSramBytes[0x38C] |= 0b1000_0000;
                    break;
                case "RedBoomerang":
                    _initialSramBytes[0x341] = 0x02;
                    _initialSramBytes[0x38C] |= 0b0100_0000;
                    break;
                case "Mushroom":
                    _initialSramBytes[0x344] = 0x01;
                    _initialSramBytes[0x38C] |= 0b0010_1000;
                    break;
                case "Powder":
                    _initialSramBytes[0x344] = 0x02;
                    _initialSramBytes[0x38C] |= 0b0001_0000;
                    break;
                case "Bombos":
                    _initialSramBytes[0x347] = 0x01;
                    break;
                case "Ether":
                    _initialSramBytes[0x348] = 0x01;
                    break;
                case "Quake":
                    _initialSramBytes[0x349] = 0x01;
                    break;
                case "Lamp":
                    _initialSramBytes[0x34A] = 0x01;
                    break;
                case "Shovel":
                    _initialSramBytes[0x34C] = 0x01;
                    _initialSramBytes[0x38C] |= 0b0000_0100;
                    break;
                case "OcarinaInactive":
                    _initialSramBytes[0x34C] = 0x02;
                    _initialSramBytes[0x38C] |= 0b0000_0010;
                    break;
                case "OcarinaActive":
                    _initialSramBytes[0x34C] = 0x03;
                    _initialSramBytes[0x38C] |= 0b0000_0001;
                    break;
                case "CaneOfSomaria":
                    _initialSramBytes[0x350] = 0x01;
                    break;
                case "Bottle":
                    for (int i = _initialSramBytes[0x34F]; i < Math.Min(_initialSramBytes[0x34F] + count, 4); i++)
                    {
                        _initialSramBytes[0x35C + i] = 0x02;
                        _initialSramBytes[0x34F]++;
                    }
                    break;
                case "BottleWithRedPotion":
                    for (int i = _initialSramBytes[0x34F]; i < Math.Min(_initialSramBytes[0x34F] + count, 4); i++)
                    {
                        _initialSramBytes[0x35C + i] = 0x03;
                        _initialSramBytes[0x34F]++;
                    }
                    break;
                case "BottleWithGreenPotion":
                    for (int i = _initialSramBytes[0x34F]; i < Math.Min(_initialSramBytes[0x34F] + count, 4); i++)
                    {
                        _initialSramBytes[0x35C + i] = 0x04;
                        _initialSramBytes[0x34F]++;
                    }
                    break;
                case "BottleWithBluePotion":
                    for (int i = _initialSramBytes[0x34F]; i < Math.Min(_initialSramBytes[0x34F] + count, 4); i++)
                    {
                        _initialSramBytes[0x35C + i] = 0x05;
                        _initialSramBytes[0x34F]++;
                    }
                    break;
                case "BottleWithFairy":
                    for (int i = _initialSramBytes[0x34F]; i < Math.Min(_initialSramBytes[0x34F] + count, 4); i++)
                    {
                        _initialSramBytes[0x35C + i] = 0x06;
                        _initialSramBytes[0x34F]++;
                    }
                    break;
                case "BottleWithBee":
                    for (int i = _initialSramBytes[0x34F]; i < Math.Min(_initialSramBytes[0x34F] + count, 4); i++)
                    {
                        _initialSramBytes[0x35C + i] = 0x07;
                        _initialSramBytes[0x34F]++;
                    }
                    break;
                case "BottleWithGoldBee":
                    for (int i = _initialSramBytes[0x34F]; i < Math.Min(_initialSramBytes[0x34F] + count, 4); i++)
                    {
                        _initialSramBytes[0x35C + i] = 0x08;
                        _initialSramBytes[0x34F]++;
                    }
                    break;
                case "CaneOfByrna":
                    _initialSramBytes[0x351] = 0x01;
                    break;
                case "Cape":
                    _initialSramBytes[0x352] = 0x01;
                    break;
                case "MagicMirror":
                    _initialSramBytes[0x353] = 0x02;
                    break;
                case "PowerGlove":
                    _initialSramBytes[0x354] = 0x01;
                    break;
                case "TitansMitt":
                    _initialSramBytes[0x354] = 0x02;
                    break;
                case "BookOfMudora":
                    _initialSramBytes[0x34E] = 0x01;
                    break;
                case "Flippers":
                    _initialSramBytes[0x356] = 0x01;
                    _initialSramBytes[0x379] |= 0b0000_0010;
                    break;
                case "MoonPearl":
                    _initialSramBytes[0x357] = 0x01;
                    break;
                case "BugCatchingNet":
                    _initialSramBytes[0x34D] = 0x01;
                    break;
                case "BlueMail":
                    _initialSramBytes[0x35B] = 0x01;
                    _initialSramBytes[0x46E] = 0x01;
                    break;
                case "RedMail":
                    _initialSramBytes[0x35B] = 0x02;
                    _initialSramBytes[0x46E] = 0x02;
                    break;
                case "Bomb":
                    _initialSramBytes[0x343] = (byte)Math.Min(_initialSramBytes[0x343] + count, 99);
                    _initialSramBytes[0x38D] |= 0b0000_0010;
                    break;
                case "ThreeBombs":
                    _initialSramBytes[0x343] = (byte)Math.Min(_initialSramBytes[0x343] + 3 * count, 99);
                    _initialSramBytes[0x38D] |= 0b0000_0010;
                    break;
                case "TenBombs":
                    _initialSramBytes[0x343] = (byte)Math.Min(_initialSramBytes[0x343] + 10 * count, 99);
                    _initialSramBytes[0x38D] |= 0b0000_0010;
                    break;
                case "OneRupee":
                    startingRupees += 1 * count;
                    break;
                case "FiveRupees":
                    startingRupees += 5 * count;
                    break;
                case "TwentyRupees":
                case "TwentyRupees2":
                    startingRupees += 20 * count;
                    break;
                case "FiftyRupees":
                    startingRupees += 50 * count;
                    break;
                case "OneHundredRupees":
                    startingRupees += 100 * count;
                    break;
                case "ThreeHundredRupees":
                    startingRupees += 300 * count;
                    break;
                case "PendantOfCourage":
                    _initialSramBytes[0x374] |= 0b0000_0100;
                    _initialSramBytes[0x429] = (byte)Math.Min(_initialSramBytes[0x429] + count, 3);
                    break;
                case "PendantOfWisdom":
                    _initialSramBytes[0x374] |= 0b0000_0001;
                    _initialSramBytes[0x429] = (byte)Math.Min(_initialSramBytes[0x429] + count, 3);
                    break;
                case "PendantOfPower":
                    _initialSramBytes[0x374] |= 0b0000_0010;
                    _initialSramBytes[0x429] = (byte)Math.Min(_initialSramBytes[0x429] + count, 3);
                    break;
                case "HeartContainerNoAnimation":
                case "BossHeartContainer":
                case "HeartContainer":
                    _initialSramBytes[0x36C] = (byte)Math.Min(_initialSramBytes[0x36C] + 0x08 * count, 0xA0);
                    _initialSramBytes[0x36D] = (byte)Math.Min(_initialSramBytes[0x36D] + 0x08 * count, 0xA0);
                    break;
                case "PieceOfHeart":
                    _initialSramBytes[0x36B] += (byte)count;
                    if (_initialSramBytes[0x36B] >= 4)
                    {
                        _initialSramBytes[0x36C] = (byte)Math.Min(_initialSramBytes[0x36C] + 0x08 * (byte)(_initialSramBytes[0x36B] / 4), 0xA0);
                        _initialSramBytes[0x36B] %= 4;
                    }
                    break;
                case "Heart":
                    _initialSramBytes[0x36D] = (byte)Math.Min(_initialSramBytes[0x36D] + 0x08 * count, 0xA0);
                    break;
                case "Arrow":
                    _initialSramBytes[0x377] = (byte)Math.Min(_initialSramBytes[0x377] + count, 99);
                    break;
                case "TenArrows":
                    _initialSramBytes[0x377] = (byte)Math.Min(_initialSramBytes[0x377] + 10 * count, 99);
                    break;
                case "SmallMagic":
                    _initialSramBytes[0x36E] = (byte)Math.Min(_initialSramBytes[0x36E] + 0x10 * count, 0x80);
                    break;
                case "PegasusBoots":
                    _initialSramBytes[0x355] = 0x01;
                    _initialSramBytes[0x379] |= 0b00000100;
                    break;
                case "BombUpgrade5":
                    startingBombCapacity += (byte)(5 * count);
                    break;
                case "BombUpgrade10":
                    startingBombCapacity += (byte)(10 * count);
                    break;
                case "ArrowUpgrade5":
                    startingArrowCapacity += (byte)(5 * count);
                    break;
                case "ArrowUpgrade10":
                    startingArrowCapacity += (byte)(10 * count);
                    break;
                case "HalfMagic":
                    _initialSramBytes[0x37B] = 0x01;
                    break;
                case "QuarterMagic":
                    _initialSramBytes[0x37B] = 0x02;
                    break;
                case "ProgressiveSword":
                    _initialSramBytes[0x359] = (byte)Math.Min(_initialSramBytes[0x359] + count, 4);
                    break;
                case "ProgressiveShield":
                    _initialSramBytes[0x35A] = (byte)Math.Min(_initialSramBytes[0x35A] + count, 3);
                    break;
                case "ProgressiveArmor":
                    _initialSramBytes[0x35B] = (byte)Math.Min(_initialSramBytes[0x35B] + count, 2);
                    break;
                case "ProgressiveGlove":
                    _initialSramBytes[0x354] = (byte)Math.Min(_initialSramBytes[0x354] + count, 2);
                    break;
                case "MapLW":
                    _initialSramBytes[0x368] |= 0b0000_0001;
                    break;
                case "MapDW":
                    _initialSramBytes[0x368] |= 0b0000_0010;
                    break;
                case "MapA2":
                    _initialSramBytes[0x368] |= 0b0000_0100;
                    break;
                case "MapD7":
                    _initialSramBytes[0x368] |= 0b0000_1000;
                    break;
                case "MapD4":
                    _initialSramBytes[0x368] |= 0b0001_0000;
                    break;
                case "MapP3":
                    _initialSramBytes[0x368] |= 0b0010_0000;
                    break;
                case "MapD5":
                    _initialSramBytes[0x368] |= 0b0100_0000;
                    break;
                case "MapD3":
                    _initialSramBytes[0x368] |= 0b1000_0000;
                    break;
                case "MapD6":
                    _initialSramBytes[0x369] |= 0b0000_0001;
                    break;
                case "MapD1":
                    _initialSramBytes[0x369] |= 0b0000_0010;
                    break;
                case "MapD2":
                    _initialSramBytes[0x369] |= 0b0000_0100;
                    break;
                case "MapA1":
                    _initialSramBytes[0x369] |= 0b0000_1000;
                    break;
                case "MapP2":
                    _initialSramBytes[0x369] |= 0b0001_0000;
                    break;
                case "MapP1":
                    _initialSramBytes[0x369] |= 0b0010_0000;
                    break;
                case "MapH1":
                case "MapH2":
                    _initialSramBytes[0x369] |= 0b1100_0000;
                    break;
                case "CompassA2":
                    _initialSramBytes[0x364] |= 0b0000_0100;
                    break;
                case "CompassD7":
                    _initialSramBytes[0x364] |= 0b0000_1000;
                    break;
                case "CompassD4":
                    _initialSramBytes[0x364] |= 0b0001_0000;
                    break;
                case "CompassP3":
                    _initialSramBytes[0x364] |= 0b0010_0000;
                    break;
                case "CompassD5":
                    _initialSramBytes[0x364] |= 0b0100_0000;
                    break;
                case "CompassD3":
                    _initialSramBytes[0x364] |= 0b1000_0000;
                    break;
                case "CompassD6":
                    _initialSramBytes[0x365] |= 0b0000_0001;
                    break;
                case "CompassD1":
                    _initialSramBytes[0x365] |= 0b0000_0010;
                    break;
                case "CompassD2":
                    _initialSramBytes[0x365] |= 0b0000_0100;
                    break;
                case "CompassA1":
                    _initialSramBytes[0x365] |= 0b0000_1000;
                    break;
                case "CompassP2":
                    _initialSramBytes[0x365] |= 0b0001_0000;
                    break;
                case "CompassP1":
                    _initialSramBytes[0x365] |= 0b0010_0000;
                    break;
                case "CompassH1":
                case "CompassH2":
                    _initialSramBytes[0x365] |= 0b1100_0000;
                    break;
                case "BigKeyA2":
                    _initialSramBytes[0x366] |= 0b0000_0100;
                    break;
                case "BigKeyD7":
                    _initialSramBytes[0x366] |= 0b0000_1000;
                    break;
                case "BigKeyD4":
                    _initialSramBytes[0x366] |= 0b0001_0000;
                    break;
                case "BigKeyP3":
                    _initialSramBytes[0x366] |= 0b0010_0000;
                    break;
                case "BigKeyD5":
                    _initialSramBytes[0x366] |= 0b0100_0000;
                    break;
                case "BigKeyD3":
                    _initialSramBytes[0x366] |= 0b1000_0000;
                    break;
                case "BigKeyD6":
                    _initialSramBytes[0x367] |= 0b0000_0001;
                    break;
                case "BigKeyD1":
                    _initialSramBytes[0x367] |= 0b0000_0010;
                    break;
                case "BigKeyD2":
                    _initialSramBytes[0x367] |= 0b0000_0100;
                    break;
                case "BigKeyA1":
                    _initialSramBytes[0x367] |= 0b0000_1000;
                    break;
                case "BigKeyP2":
                    _initialSramBytes[0x367] |= 0b0001_0000;
                    break;
                case "BigKeyP1":
                    _initialSramBytes[0x367] |= 0b0010_0000;
                    break;
                case "BigKeyH1":
                case "BigKeyH2":
                    _initialSramBytes[0x367] |= 0b1100_0000;
                    break;
                case "KeyH1":
                case "KeyH2":
                    _initialSramBytes[0x37C] += (byte)count;
                    _initialSramBytes[0x37D] += (byte)count;
                    break;
                case "KeyP1":
                    _initialSramBytes[0x37E] += (byte)count;
                    break;
                case "KeyP2":
                    _initialSramBytes[0x37F] += (byte)count;
                    break;
                case "KeyA1":
                    _initialSramBytes[0x380] += (byte)count;
                    break;
                case "KeyD2":
                    _initialSramBytes[0x381] += (byte)count;
                    break;
                case "KeyD1":
                    _initialSramBytes[0x382] += (byte)count;
                    break;
                case "KeyD6":
                    _initialSramBytes[0x383] += (byte)count;
                    break;
                case "KeyD3":
                    _initialSramBytes[0x384] += (byte)count;
                    break;
                case "KeyD5":
                    _initialSramBytes[0x385] += (byte)count;
                    break;
                case "KeyP3":
                    _initialSramBytes[0x386] += (byte)count;
                    break;
                case "KeyD4":
                    _initialSramBytes[0x387] += (byte)count;
                    break;
                case "KeyD7":
                    _initialSramBytes[0x388] += (byte)count;
                    break;
                case "KeyA2":
                    _initialSramBytes[0x389] += (byte)count;
                    break;
                case "Crystal1":
                    _initialSramBytes[0x37A] |= 0b000_00010;
                    break;
                case "Crystal2":
                    _initialSramBytes[0x37A] |= 0b0001_0000;
                    break;
                case "Crystal3":
                    _initialSramBytes[0x37A] |= 0b0100_0000;
                    break;
                case "Crystal4":
                    _initialSramBytes[0x37A] |= 0b0010_0000;
                    break;
                case "Crystal5":
                    _initialSramBytes[0x37A] |= 0b0000_0100;
                    break;
                case "Crystal6":
                    _initialSramBytes[0x37A] |= 0b0000_0001;
                    break;
                case "Crystal7":
                    _initialSramBytes[0x37A] |= 0b0000_1000;
                    break;
            }
        }
        _initialSramBytes[0x362] = _initialSramBytes[0x360] = (byte)(startingRupees & 0xFF);
        _initialSramBytes[0x363] = _initialSramBytes[0x361] = (byte)(startingRupees >> 8);

        // Set counters and highest equipment values
        _initialSramBytes[0x476] = (byte)BitOperations.PopCount(_initialSramBytes[0x37A]);
        _initialSramBytes[0x429] = (byte)BitOperations.PopCount(_initialSramBytes[0x374]);
        _initialSramBytes[0x417] = _initialSramBytes[0x359];
        _initialSramBytes[0x422] = _initialSramBytes[0x35A];
        _initialSramBytes[0x46E] = _initialSramBytes[0x35B];

        SetValue(0x370, startingBombCapacity);
        SetValue(0x371, startingArrowCapacity);

        if (config.Weapon == WeaponOption.Swordless)
        {
            _initialSramBytes[0x359] = 0xFF;
            _initialSramBytes[0x417] = 0x00;
        }
    }

    /// <summary>Set the initial progress indicator.</summary>
    /// <param name="indicator">
    /// indicator set to 0x00 for standard, 0x02 for open + See sram.asm
    /// for further documentation.
    /// </param>
    public void SetProgressIndicator(byte indicator) => SetValue(0x3C5, indicator);

    /// <summary>Set the initial progress flags.</summary>
    /// <param name="flags">
    /// set to 0x00 for standard, 0x14 for open + See sram.asm for
    /// further documentation.
    /// </param>
    public void SetProgressFlags(byte flags) => SetValue(0x3C6, flags);

    /// <summary>Set the initial starting entrance.</summary>
    /// <param name="entrance">
    /// set to 0x00 for standard, 0x01 for open + See sram.asm for
    /// further documentation.
    /// </param>
    public void SetStartingEntrance(byte entrance) => SetValue(0x3C8, entrance);

    /// <summary>Set starting timer.</summary>
    public void SetStartingTimer(int seconds)
    {
        byte[] bytes = BitConverter.GetBytes(seconds * 60);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        _initialSramBytes[0x454] = bytes[0];
        _initialSramBytes[0x455] = bytes[1];
        _initialSramBytes[0x456] = bytes[2];
        _initialSramBytes[0x457] = bytes[3];
    }

    /// <summary>Set curtains open in swordless</summary>
    public void SetSwordlessCurtains()
    {
        SetValue(ROOM_DATA + 0x61, 0x80);
        SetValue(ROOM_DATA + 0x93, 0x80);
    }

    /// <summary>Set instant post-aga world state</summary>
    /// <param name="state">world state</param>
    public void SetInstantPostAga(StateOption state)
    {
        switch (state)
        {
            case StateOption.Standard:
                SetValue(0x3C5, 0x80);
                SetValue(OVERWORLD_DATA + 0x02, 0x20);
                break;
            case StateOption.Open:
            //case "retro":
            case StateOption.Inverted:
            default:
                SetValue(0x3C5, 0x03);
                SetValue(OVERWORLD_DATA + 0x02, 0x20);
                break;
        }
    }

    /// <summary>Gets final initial SRAM table.</summary>
    /// <exception cref="Exception">if the size exceeds <see cref="SRAM_SIZE"/></exception>
    public byte[] GetInitialSram()
    {
        int tableSize = _initialSramBytes.Length;
        if (tableSize != SRAM_SIZE)
            throw new Exception("Initial SRAM table exceeds size limit: " + tableSize);

        return _initialSramBytes;
    }
}
