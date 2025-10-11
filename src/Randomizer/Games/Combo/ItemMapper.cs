namespace Randomizer.Games.Combo;

using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaseVertex = Randomizer.Graph.Vertex;

public class ItemMapper
{
    private static readonly Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> _itemBytes = new()
    {
        ["alttp"] = new()
        {
            ["L1SwordAndShield"] = new()
            {
                ["z1"] = new byte[] { 0x30 },
                ["alttp"] = new byte[] { 0x00 },
                ["m1"] = new byte[] { 0x0B, 0x00 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x00 }
            },
            ["L2Sword"] = new()
            {
                ["z1"] = new byte[] { 0x31 },
                ["alttp"] = new byte[] { 0x01 },
                ["m1"] = new byte[] { 0x0B, 0x01 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x01 }
            },
            ["L3Sword"] = new()
            {
                ["z1"] = new byte[] { 0x32 },
                ["alttp"] = new byte[] { 0x02 },
                ["m1"] = new byte[] { 0x0B, 0x02 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x02 }
            },
            ["L4Sword"] = new()
            {
                ["z1"] = new byte[] { 0x33 },
                ["alttp"] = new byte[] { 0x03 },
                ["m1"] = new byte[] { 0x0B, 0x03 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x03 }
            },
            ["BlueShield"] = new()
            {
                ["z1"] = new byte[] { 0x34 },
                ["alttp"] = new byte[] { 0x04 },
                ["m1"] = new byte[] { 0x0B, 0x04 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x04 }
            },
            ["RedShield"] = new()
            {
                ["z1"] = new byte[] { 0x35 },
                ["alttp"] = new byte[] { 0x05 },
                ["m1"] = new byte[] { 0x0B, 0x05 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x05 }
            },
            ["MirrorShield"] = new()
            {
                ["z1"] = new byte[] { 0x36 },
                ["alttp"] = new byte[] { 0x06 },
                ["m1"] = new byte[] { 0x0B, 0x06 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x06 }
            },
            ["FireRod"] = new()
            {
                ["z1"] = new byte[] { 0x37 },
                ["alttp"] = new byte[] { 0x07 },
                ["m1"] = new byte[] { 0x0B, 0x07 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x07 }
            },
            ["IceRod"] = new()
            {
                ["z1"] = new byte[] { 0x38 },
                ["alttp"] = new byte[] { 0x08 },
                ["m1"] = new byte[] { 0x0B, 0x08 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x08 }
            },
            ["Hammer"] = new()
            {
                ["z1"] = new byte[] { 0x39 },
                ["alttp"] = new byte[] { 0x09 },
                ["m1"] = new byte[] { 0x0B, 0x09 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x09 }
            },
            ["Hookshot"] = new()
            {
                ["z1"] = new byte[] { 0x3a },
                ["alttp"] = new byte[] { 0x0a },
                ["m1"] = new byte[] { 0x0B, 0x0a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x0a }
            },
            ["Bow"] = new()
            {
                ["z1"] = new byte[] { 0x3b },
                ["alttp"] = new byte[] { 0x0b },
                ["m1"] = new byte[] { 0x0B, 0x0b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x0b }
            },
            ["Boomerang"] = new()
            {
                ["z1"] = new byte[] { 0x3c },
                ["alttp"] = new byte[] { 0x0c },
                ["m1"] = new byte[] { 0x0B, 0x0c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x0c }
            },
            ["Powder"] = new()
            {
                ["z1"] = new byte[] { 0x3d },
                ["alttp"] = new byte[] { 0x0d },
                ["m1"] = new byte[] { 0x0B, 0x0d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x0d }
            },
            ["Bee"] = new()
            {
                ["z1"] = new byte[] { 0x3e },
                ["alttp"] = new byte[] { 0x0e },
                ["m1"] = new byte[] { 0x0B, 0x0e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x0e }
            },
            ["Bombos"] = new()
            {
                ["z1"] = new byte[] { 0x3f },
                ["alttp"] = new byte[] { 0x0f },
                ["m1"] = new byte[] { 0x0B, 0x0f },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x0f }
            },
            ["Ether"] = new()
            {
                ["z1"] = new byte[] { 0x40 },
                ["alttp"] = new byte[] { 0x10 },
                ["m1"] = new byte[] { 0x0B, 0x10 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x10 }
            },
            ["Quake"] = new()
            {
                ["z1"] = new byte[] { 0x41 },
                ["alttp"] = new byte[] { 0x11 },
                ["m1"] = new byte[] { 0x0B, 0x11 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x11 }
            },
            ["Lamp"] = new()
            {
                ["z1"] = new byte[] { 0x42 },
                ["alttp"] = new byte[] { 0x12 },
                ["m1"] = new byte[] { 0x0B, 0x12 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x12 }
            },
            ["Shovel"] = new()
            {
                ["z1"] = new byte[] { 0x43 },
                ["alttp"] = new byte[] { 0x13 },
                ["m1"] = new byte[] { 0x0B, 0x13 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x13 }
            },
            ["OcarinaInactive"] = new()
            {
                ["z1"] = new byte[] { 0x44 },
                ["alttp"] = new byte[] { 0x14 },
                ["m1"] = new byte[] { 0x0B, 0x14 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x14 }
            },
            ["CaneOfSomaria"] = new()
            {
                ["z1"] = new byte[] { 0x45 },
                ["alttp"] = new byte[] { 0x15 },
                ["m1"] = new byte[] { 0x0B, 0x15 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x15 }
            },
            ["Bottle"] = new()
            {
                ["z1"] = new byte[] { 0x46 },
                ["alttp"] = new byte[] { 0x16 },
                ["m1"] = new byte[] { 0x0B, 0x16 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x16 }
            },
            ["FairyBottle"] = new()
            {
                ["z1"] = new byte[] { 0x46 },
                ["alttp"] = new byte[] { 0x16 },
                ["m1"] = new byte[] { 0x0B, 0x16 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x16 }
            },
            ["PieceOfHeart"] = new()
            {
                ["z1"] = new byte[] { 0x47 },
                ["alttp"] = new byte[] { 0x17 },
                ["m1"] = new byte[] { 0x0B, 0x17 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x17 }
            },
            ["CaneOfByrna"] = new()
            {
                ["z1"] = new byte[] { 0x48 },
                ["alttp"] = new byte[] { 0x18 },
                ["m1"] = new byte[] { 0x0B, 0x18 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x18 }
            },
            ["Cape"] = new()
            {
                ["z1"] = new byte[] { 0x49 },
                ["alttp"] = new byte[] { 0x19 },
                ["m1"] = new byte[] { 0x0B, 0x19 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x19 }
            },
            ["MagicMirror"] = new()
            {
                ["z1"] = new byte[] { 0x4a },
                ["alttp"] = new byte[] { 0x1a },
                ["m1"] = new byte[] { 0x0B, 0x1a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x1a }
            },
            ["PowerGlove"] = new()
            {
                ["z1"] = new byte[] { 0x4b },
                ["alttp"] = new byte[] { 0x1b },
                ["m1"] = new byte[] { 0x0B, 0x1b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x1b }
            },
            ["TitansMitt"] = new()
            {
                ["z1"] = new byte[] { 0x4c },
                ["alttp"] = new byte[] { 0x1c },
                ["m1"] = new byte[] { 0x0B, 0x1c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x1c }
            },
            ["BookOfMudora"] = new()
            {
                ["z1"] = new byte[] { 0x4d },
                ["alttp"] = new byte[] { 0x1d },
                ["m1"] = new byte[] { 0x0B, 0x1d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x1d }
            },
            ["Flippers"] = new()
            {
                ["z1"] = new byte[] { 0x4e },
                ["alttp"] = new byte[] { 0x1e },
                ["m1"] = new byte[] { 0x0B, 0x1e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x1e }
            },
            ["MoonPearl"] = new()
            {
                ["z1"] = new byte[] { 0x4f },
                ["alttp"] = new byte[] { 0x1f },
                ["m1"] = new byte[] { 0x0B, 0x1f },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x1f }
            },
            ["BugCatchingNet"] = new()
            {
                ["z1"] = new byte[] { 0x51 },
                ["alttp"] = new byte[] { 0x21 },
                ["m1"] = new byte[] { 0x0B, 0x21 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x21 }
            },
            ["BlueMail"] = new()
            {
                ["z1"] = new byte[] { 0x52 },
                ["alttp"] = new byte[] { 0x22 },
                ["m1"] = new byte[] { 0x0B, 0x22 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x22 }
            },
            ["RedMail"] = new()
            {
                ["z1"] = new byte[] { 0x53 },
                ["alttp"] = new byte[] { 0x23 },
                ["m1"] = new byte[] { 0x0B, 0x23 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x23 }
            },
            ["Key"] = new()
            {
                ["z1"] = new byte[] { 0x54 },
                ["alttp"] = new byte[] { 0x24 },
                ["m1"] = new byte[] { 0x0B, 0x24 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x24 }
            },
            ["Compass"] = new()
            {
                ["z1"] = new byte[] { 0x55 },
                ["alttp"] = new byte[] { 0x25 },
                ["m1"] = new byte[] { 0x0B, 0x25 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x25 }
            },
            ["HeartContainerNoAnimation"] = new()
            {
                ["z1"] = new byte[] { 0x56 },
                ["alttp"] = new byte[] { 0x26 },
                ["m1"] = new byte[] { 0x0B, 0x26 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x26 }
            },
            ["Bomb"] = new()
            {
                ["z1"] = new byte[] { 0x57 },
                ["alttp"] = new byte[] { 0x27 },
                ["m1"] = new byte[] { 0x0B, 0x27 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x27 }
            },
            ["ThreeBombs"] = new()
            {
                ["z1"] = new byte[] { 0x58 },
                ["alttp"] = new byte[] { 0x28 },
                ["m1"] = new byte[] { 0x0B, 0x28 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x28 }
            },
            ["Mushroom"] = new()
            {
                ["z1"] = new byte[] { 0x59 },
                ["alttp"] = new byte[] { 0x29 },
                ["m1"] = new byte[] { 0x0B, 0x29 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x29 }
            },
            ["RedBoomerang"] = new()
            {
                ["z1"] = new byte[] { 0x5a },
                ["alttp"] = new byte[] { 0x2a },
                ["m1"] = new byte[] { 0x0B, 0x2a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x2a }
            },
            ["BottleWithRedPotion"] = new()
            {
                ["z1"] = new byte[] { 0x5b },
                ["alttp"] = new byte[] { 0x2b },
                ["m1"] = new byte[] { 0x0B, 0x2b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x2b }
            },
            ["FairyBottleWithRedPotion"] = new()
            {
                ["z1"] = new byte[] { 0x5b },
                ["alttp"] = new byte[] { 0x2b },
                ["m1"] = new byte[] { 0x0B, 0x2b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x2b }
            },
            ["BottleWithGreenPotion"] = new()
            {
                ["z1"] = new byte[] { 0x5c },
                ["alttp"] = new byte[] { 0x2c },
                ["m1"] = new byte[] { 0x0B, 0x2c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x2c }
            },
            ["FairyBottleWithGreenPotion"] = new()
            {
                ["z1"] = new byte[] { 0x5c },
                ["alttp"] = new byte[] { 0x2c },
                ["m1"] = new byte[] { 0x0B, 0x2c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x2c }
            },
            ["BottleWithBluePotion"] = new()
            {
                ["z1"] = new byte[] { 0x5d },
                ["alttp"] = new byte[] { 0x2d },
                ["m1"] = new byte[] { 0x0B, 0x2d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x2d }
            },
            ["FairyBottleWithBluePotion"] = new()
            {
                ["z1"] = new byte[] { 0x5d },
                ["alttp"] = new byte[] { 0x2d },
                ["m1"] = new byte[] { 0x0B, 0x2d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x2d }
            },
            ["RedPotion"] = new()
            {
                ["z1"] = new byte[] { 0x5e },
                ["alttp"] = new byte[] { 0x2e },
                ["m1"] = new byte[] { 0x0B, 0x2e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x2e }
            },
            ["GreenPotion"] = new()
            {
                ["z1"] = new byte[] { 0x5f },
                ["alttp"] = new byte[] { 0x2f },
                ["m1"] = new byte[] { 0x0B, 0x2f },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x2f }
            },
            ["BluePotion"] = new()
            {
                ["z1"] = new byte[] { 0x60 },
                ["alttp"] = new byte[] { 0x30 },
                ["m1"] = new byte[] { 0x0B, 0x30 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x30 }
            },
            ["TenBombs"] = new()
            {
                ["z1"] = new byte[] { 0x61 },
                ["alttp"] = new byte[] { 0x31 },
                ["m1"] = new byte[] { 0x0B, 0x31 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x31 }
            },
            ["BigKey"] = new()
            {
                ["z1"] = new byte[] { 0x62 },
                ["alttp"] = new byte[] { 0x32 },
                ["m1"] = new byte[] { 0x0B, 0x32 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x32 }
            },
            ["Map"] = new()
            {
                ["z1"] = new byte[] { 0x63 },
                ["alttp"] = new byte[] { 0x33 },
                ["m1"] = new byte[] { 0x0B, 0x33 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x33 }
            },
            ["OneRupee"] = new()
            {
                ["z1"] = new byte[] { 0x64 },
                ["alttp"] = new byte[] { 0x34 },
                ["m1"] = new byte[] { 0x0B, 0x34 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x34 }
            },
            ["FiveRupees"] = new()
            {
                ["z1"] = new byte[] { 0x65 },
                ["alttp"] = new byte[] { 0x35 },
                ["m1"] = new byte[] { 0x0B, 0x35 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x35 }
            },
            ["TwentyRupees"] = new()
            {
                ["z1"] = new byte[] { 0x66 },
                ["alttp"] = new byte[] { 0x36 },
                ["m1"] = new byte[] { 0x0B, 0x36 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x36 }
            },
            ["BowAndArrows"] = new()
            {
                ["z1"] = new byte[] { 0x6a },
                ["alttp"] = new byte[] { 0x3a },
                ["m1"] = new byte[] { 0x0B, 0x3a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x3a }
            },
            ["BowAndSilverArrows"] = new()
            {
                ["z1"] = new byte[] { 0x6b },
                ["alttp"] = new byte[] { 0x3b },
                ["m1"] = new byte[] { 0x0B, 0x3b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x3b }
            },
            ["BottleWithBee"] = new()
            {
                ["z1"] = new byte[] { 0x6c },
                ["alttp"] = new byte[] { 0x3c },
                ["m1"] = new byte[] { 0x0B, 0x3c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x3c }
            },
            ["FairyBottleWithBee"] = new()
            {
                ["z1"] = new byte[] { 0x6c },
                ["alttp"] = new byte[] { 0x3c },
                ["m1"] = new byte[] { 0x0B, 0x3c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x3c }
            },
            ["BottleWithFairy"] = new()
            {
                ["z1"] = new byte[] { 0x6d },
                ["alttp"] = new byte[] { 0x3d },
                ["m1"] = new byte[] { 0x0B, 0x3d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x3d }
            },
            ["FairyBottleWithFairy"] = new()
            {
                ["z1"] = new byte[] { 0x6d },
                ["alttp"] = new byte[] { 0x3d },
                ["m1"] = new byte[] { 0x0B, 0x3d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x3d }
            },
            ["BossHeartContainer"] = new()
            {
                ["z1"] = new byte[] { 0x6e },
                ["alttp"] = new byte[] { 0x3e },
                ["m1"] = new byte[] { 0x0B, 0x3e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x3e }
            },
            ["HeartContainer"] = new()
            {
                ["z1"] = new byte[] { 0x6f },
                ["alttp"] = new byte[] { 0x3f },
                ["m1"] = new byte[] { 0x0B, 0x3f },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x3f }
            },
            ["OneHundredRupees"] = new()
            {
                ["z1"] = new byte[] { 0x70 },
                ["alttp"] = new byte[] { 0x40 },
                ["m1"] = new byte[] { 0x0B, 0x40 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x40 }
            },
            ["FiftyRupees"] = new()
            {
                ["z1"] = new byte[] { 0x71 },
                ["alttp"] = new byte[] { 0x41 },
                ["m1"] = new byte[] { 0x0B, 0x41 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x41 }
            },
            ["Heart"] = new()
            {
                ["z1"] = new byte[] { 0x72 },
                ["alttp"] = new byte[] { 0x42 },
                ["m1"] = new byte[] { 0x0B, 0x42 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x42 }
            },
            ["Arrow"] = new()
            {
                ["z1"] = new byte[] { 0x73 },
                ["alttp"] = new byte[] { 0x43 },
                ["m1"] = new byte[] { 0x0B, 0x43 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x43 }
            },
            ["ShopArrow"] = new()
            {
                ["z1"] = new byte[] { 0x73 },
                ["alttp"] = new byte[] { 0x43 },
                ["m1"] = new byte[] { 0x0B, 0x43 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x43 }
            },
            ["TenArrows"] = new()
            {
                ["z1"] = new byte[] { 0x74 },
                ["alttp"] = new byte[] { 0x44 },
                ["m1"] = new byte[] { 0x0B, 0x44 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x44 }
            },
            ["SmallMagic"] = new()
            {
                ["z1"] = new byte[] { 0x75 },
                ["alttp"] = new byte[] { 0x45 },
                ["m1"] = new byte[] { 0x0B, 0x45 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x45 }
            },
            ["ThreeHundredRupees"] = new()
            {
                ["z1"] = new byte[] { 0x76 },
                ["alttp"] = new byte[] { 0x46 },
                ["m1"] = new byte[] { 0x0B, 0x46 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x46 }
            },
            ["TwentyRupees2"] = new()
            {
                ["z1"] = new byte[] { 0x77 },
                ["alttp"] = new byte[] { 0x47 },
                ["m1"] = new byte[] { 0x0B, 0x47 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x47 }
            },
            ["BottleWithGoldBee"] = new()
            {
                ["z1"] = new byte[] { 0x78 },
                ["alttp"] = new byte[] { 0x48 },
                ["m1"] = new byte[] { 0x0B, 0x48 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x48 }
            },
            ["FairyBottleWithGoldBee"] = new()
            {
                ["z1"] = new byte[] { 0x78 },
                ["alttp"] = new byte[] { 0x48 },
                ["m1"] = new byte[] { 0x0B, 0x48 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x48 }
            },
            ["L1Sword"] = new()
            {
                ["z1"] = new byte[] { 0x79 },
                ["alttp"] = new byte[] { 0x49 },
                ["m1"] = new byte[] { 0x0B, 0x49 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x49 }
            },
            ["OcarinaActive"] = new()
            {
                ["z1"] = new byte[] { 0x7a },
                ["alttp"] = new byte[] { 0x4a },
                ["m1"] = new byte[] { 0x0B, 0x4a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x4a }
            },
            ["PegasusBoots"] = new()
            {
                ["z1"] = new byte[] { 0x7b },
                ["alttp"] = new byte[] { 0x4b },
                ["m1"] = new byte[] { 0x0B, 0x4b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x4b }
            },
            ["BombUpgrade50"] = new()
            {
                ["z1"] = new byte[] { 0x7c },
                ["alttp"] = new byte[] { 0x4c },
                ["m1"] = new byte[] { 0x0B, 0x4c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x4c }
            },
            ["ArrowUpgrade70"] = new()
            {
                ["z1"] = new byte[] { 0x7d },
                ["alttp"] = new byte[] { 0x4d },
                ["m1"] = new byte[] { 0x0B, 0x4d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x4d }
            },
            ["HalfMagic"] = new()
            {
                ["z1"] = new byte[] { 0x7e },
                ["alttp"] = new byte[] { 0x4e },
                ["m1"] = new byte[] { 0x0B, 0x4e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x4e }
            },
            ["QuarterMagic"] = new()
            {
                ["z1"] = new byte[] { 0x7f },
                ["alttp"] = new byte[] { 0x4f },
                ["m1"] = new byte[] { 0x0B, 0x4f },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x4f }
            },
            ["MasterSword"] = new()
            {
                ["z1"] = new byte[] { 0x80 },
                ["alttp"] = new byte[] { 0x50 },
                ["m1"] = new byte[] { 0x0B, 0x50 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x50 }
            },
            ["BombUpgrade5"] = new()
            {
                ["z1"] = new byte[] { 0x81 },
                ["alttp"] = new byte[] { 0x51 },
                ["m1"] = new byte[] { 0x0B, 0x51 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x51 }
            },
            ["BombUpgrade10"] = new()
            {
                ["z1"] = new byte[] { 0x82 },
                ["alttp"] = new byte[] { 0x52 },
                ["m1"] = new byte[] { 0x0B, 0x52 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x52 }
            },
            ["ArrowUpgrade5"] = new()
            {
                ["z1"] = new byte[] { 0x83 },
                ["alttp"] = new byte[] { 0x53 },
                ["m1"] = new byte[] { 0x0B, 0x53 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x53 }
            },
            ["ArrowUpgrade10"] = new()
            {
                ["z1"] = new byte[] { 0x84 },
                ["alttp"] = new byte[] { 0x54 },
                ["m1"] = new byte[] { 0x0B, 0x54 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x54 }
            },
            ["Programmable1"] = new()
            {
                ["z1"] = new byte[] { 0x85 },
                ["alttp"] = new byte[] { 0x55 },
                ["m1"] = new byte[] { 0x0B, 0x55 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x55 }
            },
            ["Programmable2"] = new()
            {
                ["z1"] = new byte[] { 0x86 },
                ["alttp"] = new byte[] { 0x56 },
                ["m1"] = new byte[] { 0x0B, 0x56 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x56 }
            },
            ["Programmable3"] = new()
            {
                ["z1"] = new byte[] { 0x87 },
                ["alttp"] = new byte[] { 0x57 },
                ["m1"] = new byte[] { 0x0B, 0x57 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x57 }
            },
            ["SilverArrowUpgrade"] = new()
            {
                ["z1"] = new byte[] { 0x88 },
                ["alttp"] = new byte[] { 0x58 },
                ["m1"] = new byte[] { 0x0B, 0x58 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x58 }
            },
            ["Rupoor"] = new()
            {
                ["z1"] = new byte[] { 0x89 },
                ["alttp"] = new byte[] { 0x59 },
                ["m1"] = new byte[] { 0x0B, 0x59 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x59 }
            },
            ["Nothing"] = new()
            {
                ["z1"] = new byte[] { 0x2f },
                ["alttp"] = new byte[] { 0x5a },
                ["m1"] = new byte[] { 0x0B, 0x5a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x5a }
            },
            ["RedClock"] = new()
            {
                ["z1"] = new byte[] { 0x8b },
                ["alttp"] = new byte[] { 0x5b },
                ["m1"] = new byte[] { 0x0B, 0x5b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x5b }
            },
            ["BlueClock"] = new()
            {
                ["z1"] = new byte[] { 0x8c },
                ["alttp"] = new byte[] { 0x5c },
                ["m1"] = new byte[] { 0x0B, 0x5c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x5c }
            },
            ["GreenClock"] = new()
            {
                ["z1"] = new byte[] { 0x8d },
                ["alttp"] = new byte[] { 0x5d },
                ["m1"] = new byte[] { 0x0B, 0x5d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x5d }
            },
            ["ProgressiveSword"] = new()
            {
                ["z1"] = new byte[] { 0x8e },
                ["alttp"] = new byte[] { 0x5e },
                ["m1"] = new byte[] { 0x0B, 0x5e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x5e }
            },
            ["UncleSword"] = new()
            {
                ["z1"] = new byte[] { 0x8e },
                ["alttp"] = new byte[] { 0x5e },
                ["m1"] = new byte[] { 0x0B, 0x5e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x5e }
            },
            ["ProgressiveShield"] = new()
            {
                ["z1"] = new byte[] { 0x8f },
                ["alttp"] = new byte[] { 0x5f },
                ["m1"] = new byte[] { 0x0B, 0x5f },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x5f }
            },
            ["ProgressiveArmor"] = new()
            {
                ["z1"] = new byte[] { 0x90 },
                ["alttp"] = new byte[] { 0x60 },
                ["m1"] = new byte[] { 0x0B, 0x60 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x60 }
            },
            ["ProgressiveGlove"] = new()
            {
                ["z1"] = new byte[] { 0x91 },
                ["alttp"] = new byte[] { 0x61 },
                ["m1"] = new byte[] { 0x0B, 0x61 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x61 }
            },
            ["ProgressiveBow"] = new()
            {
                ["z1"] = new byte[] { 0x94 },
                ["alttp"] = new byte[] { 0x64 },
                ["m1"] = new byte[] { 0x0B, 0x64 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x64 }
            },
            ["ProgressiveBowAlternate"] = new()
            {
                ["z1"] = new byte[] { 0x95 },
                ["alttp"] = new byte[] { 0x65 },
                ["m1"] = new byte[] { 0x0B, 0x65 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x65 }
            },
            ["Triforce"] = new()
            {
                ["z1"] = new byte[] { 0x9a },
                ["alttp"] = new byte[] { 0x6a },
                ["m1"] = new byte[] { 0x0B, 0x6a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x6a }
            },
            ["PowerStar"] = new()
            {
                ["z1"] = new byte[] { 0x9b },
                ["alttp"] = new byte[] { 0x6b },
                ["m1"] = new byte[] { 0x0B, 0x6b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x6b }
            },
            ["TriforcePiece"] = new()
            {
                ["z1"] = new byte[] { 0x9b },
                ["alttp"] = new byte[] { 0x6b },
                ["m1"] = new byte[] { 0x0B, 0x6b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x6b }
            },
            ["MapA2"] = new()
            {
                ["z1"] = new byte[] { 0xa2 },
                ["alttp"] = new byte[] { 0x72 },
                ["m1"] = new byte[] { 0x0B, 0x72 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x72 }
            },
            ["MapD7"] = new()
            {
                ["z1"] = new byte[] { 0xa3 },
                ["alttp"] = new byte[] { 0x73 },
                ["m1"] = new byte[] { 0x0B, 0x73 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x73 }
            },
            ["MapD4"] = new()
            {
                ["z1"] = new byte[] { 0xa4 },
                ["alttp"] = new byte[] { 0x74 },
                ["m1"] = new byte[] { 0x0B, 0x74 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x74 }
            },
            ["MapP3"] = new()
            {
                ["z1"] = new byte[] { 0xa5 },
                ["alttp"] = new byte[] { 0x75 },
                ["m1"] = new byte[] { 0x0B, 0x75 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x75 }
            },
            ["MapD5"] = new()
            {
                ["z1"] = new byte[] { 0xa6 },
                ["alttp"] = new byte[] { 0x76 },
                ["m1"] = new byte[] { 0x0B, 0x76 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x76 }
            },
            ["MapD3"] = new()
            {
                ["z1"] = new byte[] { 0xa7 },
                ["alttp"] = new byte[] { 0x77 },
                ["m1"] = new byte[] { 0x0B, 0x77 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x77 }
            },
            ["MapD6"] = new()
            {
                ["z1"] = new byte[] { 0xa8 },
                ["alttp"] = new byte[] { 0x78 },
                ["m1"] = new byte[] { 0x0B, 0x78 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x78 }
            },
            ["MapD1"] = new()
            {
                ["z1"] = new byte[] { 0xa9 },
                ["alttp"] = new byte[] { 0x79 },
                ["m1"] = new byte[] { 0x0B, 0x79 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x79 }
            },
            ["MapD2"] = new()
            {
                ["z1"] = new byte[] { 0xaa },
                ["alttp"] = new byte[] { 0x7a },
                ["m1"] = new byte[] { 0x0B, 0x7a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x7a }
            },
            ["MapP2"] = new()
            {
                ["z1"] = new byte[] { 0xac },
                ["alttp"] = new byte[] { 0x7c },
                ["m1"] = new byte[] { 0x0B, 0x7c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x7c }
            },
            ["MapP1"] = new()
            {
                ["z1"] = new byte[] { 0xad },
                ["alttp"] = new byte[] { 0x7d },
                ["m1"] = new byte[] { 0x0B, 0x7d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x7d }
            },
            ["MapH2"] = new()
            {
                ["z1"] = new byte[] { 0xaf },
                ["alttp"] = new byte[] { 0x7f },
                ["m1"] = new byte[] { 0x0B, 0x7f },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x7f }
            },
            ["CompassA2"] = new()
            {
                ["z1"] = new byte[] { 0xb2 },
                ["alttp"] = new byte[] { 0x82 },
                ["m1"] = new byte[] { 0x0B, 0x82 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x82 }
            },
            ["CompassD7"] = new()
            {
                ["z1"] = new byte[] { 0xb3 },
                ["alttp"] = new byte[] { 0x83 },
                ["m1"] = new byte[] { 0x0B, 0x83 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x83 }
            },
            ["CompassD4"] = new()
            {
                ["z1"] = new byte[] { 0xb4 },
                ["alttp"] = new byte[] { 0x84 },
                ["m1"] = new byte[] { 0x0B, 0x84 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x84 }
            },
            ["CompassP3"] = new()
            {
                ["z1"] = new byte[] { 0xb5 },
                ["alttp"] = new byte[] { 0x85 },
                ["m1"] = new byte[] { 0x0B, 0x85 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x85 }
            },
            ["CompassD5"] = new()
            {
                ["z1"] = new byte[] { 0xb6 },
                ["alttp"] = new byte[] { 0x86 },
                ["m1"] = new byte[] { 0x0B, 0x86 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x86 }
            },
            ["CompassD3"] = new()
            {
                ["z1"] = new byte[] { 0xb7 },
                ["alttp"] = new byte[] { 0x87 },
                ["m1"] = new byte[] { 0x0B, 0x87 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x87 }
            },
            ["CompassD6"] = new()
            {
                ["z1"] = new byte[] { 0xb8 },
                ["alttp"] = new byte[] { 0x88 },
                ["m1"] = new byte[] { 0x0B, 0x88 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x88 }
            },
            ["CompassD1"] = new()
            {
                ["z1"] = new byte[] { 0xb9 },
                ["alttp"] = new byte[] { 0x89 },
                ["m1"] = new byte[] { 0x0B, 0x89 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x89 }
            },
            ["CompassD2"] = new()
            {
                ["z1"] = new byte[] { 0xba },
                ["alttp"] = new byte[] { 0x8a },
                ["m1"] = new byte[] { 0x0B, 0x8a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x8a }
            },
            ["CompassP2"] = new()
            {
                ["z1"] = new byte[] { 0xbc },
                ["alttp"] = new byte[] { 0x8c },
                ["m1"] = new byte[] { 0x0B, 0x8c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x8c }
            },
            ["CompassP1"] = new()
            {
                ["z1"] = new byte[] { 0xbd },
                ["alttp"] = new byte[] { 0x8d },
                ["m1"] = new byte[] { 0x0B, 0x8d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x8d }
            },
            ["BigKeyA2"] = new()
            {
                ["z1"] = new byte[] { 0xc2 },
                ["alttp"] = new byte[] { 0x92 },
                ["m1"] = new byte[] { 0x0B, 0x92 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x92 }
            },
            ["BigKeyD7"] = new()
            {
                ["z1"] = new byte[] { 0xc3 },
                ["alttp"] = new byte[] { 0x93 },
                ["m1"] = new byte[] { 0x0B, 0x93 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x93 }
            },
            ["BigKeyD4"] = new()
            {
                ["z1"] = new byte[] { 0xc4 },
                ["alttp"] = new byte[] { 0x94 },
                ["m1"] = new byte[] { 0x0B, 0x94 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x94 }
            },
            ["BigKeyP3"] = new()
            {
                ["z1"] = new byte[] { 0xc5 },
                ["alttp"] = new byte[] { 0x95 },
                ["m1"] = new byte[] { 0x0B, 0x95 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x95 }
            },
            ["BigKeyD5"] = new()
            {
                ["z1"] = new byte[] { 0xc6 },
                ["alttp"] = new byte[] { 0x96 },
                ["m1"] = new byte[] { 0x0B, 0x96 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x96 }
            },
            ["BigKeyD3"] = new()
            {
                ["z1"] = new byte[] { 0xc7 },
                ["alttp"] = new byte[] { 0x97 },
                ["m1"] = new byte[] { 0x0B, 0x97 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x97 }
            },
            ["BigKeyD6"] = new()
            {
                ["z1"] = new byte[] { 0xc8 },
                ["alttp"] = new byte[] { 0x98 },
                ["m1"] = new byte[] { 0x0B, 0x98 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x98 }
            },
            ["BigKeyD1"] = new()
            {
                ["z1"] = new byte[] { 0xc9 },
                ["alttp"] = new byte[] { 0x99 },
                ["m1"] = new byte[] { 0x0B, 0x99 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x99 }
            },
            ["BigKeyD2"] = new()
            {
                ["z1"] = new byte[] { 0xca },
                ["alttp"] = new byte[] { 0x9a },
                ["m1"] = new byte[] { 0x0B, 0x9a },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x9a }
            },
            ["BigKeyP2"] = new()
            {
                ["z1"] = new byte[] { 0xcc },
                ["alttp"] = new byte[] { 0x9c },
                ["m1"] = new byte[] { 0x0B, 0x9c },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x9c }
            },
            ["BigKeyP1"] = new()
            {
                ["z1"] = new byte[] { 0xcd },
                ["alttp"] = new byte[] { 0x9d },
                ["m1"] = new byte[] { 0x0B, 0x9d },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x9d }
            },
            ["KeyH2"] = new()
            {
                ["z1"] = new byte[] { 0xd0 },
                ["alttp"] = new byte[] { 0xa0 },
                ["m1"] = new byte[] { 0x0B, 0xa0 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa0 }
            },
            ["KeyH1"] = new()
            {
                ["z1"] = new byte[] { 0xd1 },
                ["alttp"] = new byte[] { 0xa1 },
                ["m1"] = new byte[] { 0x0B, 0xa1 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa1 }
            },
            ["KeyP1"] = new()
            {
                ["z1"] = new byte[] { 0xd2 },
                ["alttp"] = new byte[] { 0xa2 },
                ["m1"] = new byte[] { 0x0B, 0xa2 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa2 }
            },
            ["KeyP2"] = new()
            {
                ["z1"] = new byte[] { 0xd3 },
                ["alttp"] = new byte[] { 0xa3 },
                ["m1"] = new byte[] { 0x0B, 0xa3 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa3 }
            },
            ["KeyA1"] = new()
            {
                ["z1"] = new byte[] { 0xd4 },
                ["alttp"] = new byte[] { 0xa4 },
                ["m1"] = new byte[] { 0x0B, 0xa4 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa4 }
            },
            ["KeyD2"] = new()
            {
                ["z1"] = new byte[] { 0xd5 },
                ["alttp"] = new byte[] { 0xa5 },
                ["m1"] = new byte[] { 0x0B, 0xa5 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa5 }
            },
            ["KeyD1"] = new()
            {
                ["z1"] = new byte[] { 0xd6 },
                ["alttp"] = new byte[] { 0xa6 },
                ["m1"] = new byte[] { 0x0B, 0xa6 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa6 }
            },
            ["KeyD6"] = new()
            {
                ["z1"] = new byte[] { 0xd7 },
                ["alttp"] = new byte[] { 0xa7 },
                ["m1"] = new byte[] { 0x0B, 0xa7 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa7 }
            },
            ["KeyD3"] = new()
            {
                ["z1"] = new byte[] { 0xd8 },
                ["alttp"] = new byte[] { 0xa8 },
                ["m1"] = new byte[] { 0x0B, 0xa8 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa8 }
            },
            ["KeyD5"] = new()
            {
                ["z1"] = new byte[] { 0xd9 },
                ["alttp"] = new byte[] { 0xa9 },
                ["m1"] = new byte[] { 0x0B, 0xa9 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xa9 }
            },
            ["KeyP3"] = new()
            {
                ["z1"] = new byte[] { 0xda },
                ["alttp"] = new byte[] { 0xaa },
                ["m1"] = new byte[] { 0x0B, 0xaa },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xaa }
            },
            ["KeyD4"] = new()
            {
                ["z1"] = new byte[] { 0xdb },
                ["alttp"] = new byte[] { 0xab },
                ["m1"] = new byte[] { 0x0B, 0xab },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xab }
            },
            ["KeyD7"] = new()
            {
                ["z1"] = new byte[] { 0xdc },
                ["alttp"] = new byte[] { 0xac },
                ["m1"] = new byte[] { 0x0B, 0xac },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xac }
            },
            ["KeyA2"] = new()
            {
                ["z1"] = new byte[] { 0xdd },
                ["alttp"] = new byte[] { 0xad },
                ["m1"] = new byte[] { 0x0B, 0xad },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xad }
            },
            ["KeyGK"] = new()
            {
                ["z1"] = new byte[] { 0xdf },
                ["alttp"] = new byte[] { 0xaf },
                ["m1"] = new byte[] { 0x0B, 0xaf },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xaf }
            },
            ["ShopKey"] = new()
            {
                ["z1"] = new byte[] { 0xdf },
                ["alttp"] = new byte[] { 0xaf },
                ["m1"] = new byte[] { 0x0B, 0xaf },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xaf }
            },
            ["MireEntryBombos"] = new()
            {
                ["alttp"] = new byte[] { 0x00, 0x31, 0x80, 0x00 }
            },
            ["TurtleRockEntryBombos"] = new()
            {
                ["alttp"] = new byte[] { 0x00, 0x31, 0x90, 0x00 }
            },
            ["MireEntryEther"] = new()
            {
                ["alttp"] = new byte[] { 0x01, 0x13, 0x9f, 0xf1 }
            },
            ["TurtleRockEntryEther"] = new()
            {
                ["alttp"] = new byte[] { 0x01, 0x31, 0x98, 0x00 }
            },
            ["TurtleRockEntryQuake"] = new()
            {
                ["alttp"] = new byte[] { 0x02, 0x14, 0xef, 0xc4 }
            },
            ["MireEntryQuake"] = new()
            {
                ["alttp"] = new byte[] { 0x02, 0x31, 0x88, 0x00 }
            },
            ["PendantOfCourage"] = new()
            {
                ["alttp"] = new byte[] { 0x04, 0x38, 0x62, 0x00, 0x69, 0x37 }
            },
            ["PendantOfWisdom"] = new()
            {
                ["alttp"] = new byte[] { 0x01, 0x32, 0x60, 0x00, 0x69, 0x38 }
            },
            ["PendantOfPower"] = new()
            {
                ["alttp"] = new byte[] { 0x02, 0x34, 0x60, 0x00, 0x69, 0x39 }
            },
            ["Crystal1"] = new()
            {
                ["alttp"] = new byte[] { 0x02, 0x34, 0x64, 0x40, 0x7F, 0x20 }
            },
            ["Crystal2"] = new()
            {
                ["alttp"] = new byte[] { 0x10, 0x34, 0x64, 0x40, 0x79, 0x20 }
            },
            ["Crystal3"] = new()
            {
                ["alttp"] = new byte[] { 0x40, 0x34, 0x64, 0x40, 0x6C, 0x20 }
            },
            ["Crystal4"] = new()
            {
                ["alttp"] = new byte[] { 0x20, 0x34, 0x64, 0x40, 0x6D, 0x20 }
            },
            ["Crystal5"] = new()
            {
                ["alttp"] = new byte[] { 0x04, 0x32, 0x64, 0x40, 0x6E, 0x20 }
            },
            ["Crystal6"] = new()
            {
                ["alttp"] = new byte[] { 0x01, 0x32, 0x64, 0x40, 0x6F, 0x20 }
            },
            ["Crystal7"] = new()
            {
                ["alttp"] = new byte[] { 0x08, 0x34, 0x64, 0x40, 0x7C, 0x20 }
            }
        },
        ["sm"] = new()
        {
            ["CrateriaL1"] = new()
            {
                ["z1"] = new byte[] { 0xa0 },
                ["alttp"] = new byte[] { 0x70 },
                ["m1"] = new byte[] { 0x0b, 0x70 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x70 }
            },
            ["CrateriaL2"] = new()
            {
                ["z1"] = new byte[] { 0xa1 },
                ["alttp"] = new byte[] { 0x71 },
                ["m1"] = new byte[] { 0x0B, 0x71 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x71 }
            },
            ["CrateriaBoss"] = new()
            {
                ["z1"] = new byte[] { 0xab },
                ["alttp"] = new byte[] { 0x7b },
                ["m1"] = new byte[] { 0x0B, 0x7b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x7b }
            },
            ["MaridiaBoss"] = new()
            {
                ["z1"] = new byte[] { 0xae },
                ["alttp"] = new byte[] { 0x7e },
                ["m1"] = new byte[] { 0x0B, 0x7e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x7e }
            },
            ["BrinstarL1"] = new()
            {
                ["z1"] = new byte[] { 0xb0 },
                ["alttp"] = new byte[] { 0x80 },
                ["m1"] = new byte[] { 0x0B, 0x80 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x80 }
            },
            ["BrinstarL2"] = new()
            {
                ["z1"] = new byte[] { 0xb1 },
                ["alttp"] = new byte[] { 0x81 },
                ["m1"] = new byte[] { 0x0B, 0x81 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x81 }
            },
            ["BrinstarBoss"] = new()
            {
                ["z1"] = new byte[] { 0xbb },
                ["alttp"] = new byte[] { 0x8b },
                ["m1"] = new byte[] { 0x0B, 0x8b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x8b }
            },
            ["WreckedShipL1"] = new()
            {
                ["z1"] = new byte[] { 0xbe },
                ["alttp"] = new byte[] { 0x8e },
                ["m1"] = new byte[] { 0x0B, 0x8e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x8e }
            },
            ["WreckedShipBoss"] = new()
            {
                ["z1"] = new byte[] { 0xbf },
                ["alttp"] = new byte[] { 0x8f },
                ["m1"] = new byte[] { 0x0B, 0x8f },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x8f }
            },
            ["NorfairL1"] = new()
            {
                ["z1"] = new byte[] { 0xc0 },
                ["alttp"] = new byte[] { 0x90 },
                ["m1"] = new byte[] { 0x0B, 0x90 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x90 }
            },
            ["NorfairL2"] = new()
            {
                ["z1"] = new byte[] { 0xc1 },
                ["alttp"] = new byte[] { 0x91 },
                ["m1"] = new byte[] { 0x0B, 0x91 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x91 }
            },
            ["NorfairBoss"] = new()
            {
                ["z1"] = new byte[] { 0xcb },
                ["alttp"] = new byte[] { 0x9b },
                ["m1"] = new byte[] { 0x0B, 0x9b },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x9b }
            },
            ["MaridiaL1"] = new()
            {
                ["z1"] = new byte[] { 0xde },
                ["alttp"] = new byte[] { 0xae },
                ["m1"] = new byte[] { 0x0B, 0xae },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xae }
            },
            ["MaridiaL2"] = new()
            {
                ["z1"] = new byte[] { 0xdf },
                ["alttp"] = new byte[] { 0xaf },
                ["m1"] = new byte[] { 0x0B, 0xaf },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xaf }
            },
            ["LowerNorfairL1"] = new()
            {
                ["z1"] = new byte[] { 0xce },
                ["alttp"] = new byte[] { 0x9e },
                ["m1"] = new byte[] { 0x0B, 0x9e },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x9e }
            },
            ["LowerNorfairBoss"] = new()
            {
                ["z1"] = new byte[] { 0xcf },
                ["alttp"] = new byte[] { 0x9f },
                ["m1"] = new byte[] { 0x0B, 0x9f },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x9f }
            },
            ["Grapple"] = new()
            {
                ["z1"] = new byte[] { 0xe0 },
                ["alttp"] = new byte[] { 0xb0 },
                ["m1"] = new byte[] { 0x0B, 0xb0 },
                ["sm"] = new byte[] { 0x17, 0xEF }
            },
            ["XRayScope"] = new()
            {
                ["z1"] = new byte[] { 0xe1 },
                ["alttp"] = new byte[] { 0xb1 },
                ["m1"] = new byte[] { 0x0B, 0xb1 },
                ["sm"] = new byte[] { 0x0F, 0xEF }
            },
            ["Varia"] = new()
            {
                ["z1"] = new byte[] { 0xe2 },
                ["alttp"] = new byte[] { 0xb2 },
                ["m1"] = new byte[] { 0x0B, 0xb2 },
                ["sm"] = new byte[] { 0x07, 0xEF }
            },
            ["SpringBall"] = new()
            {
                ["z1"] = new byte[] { 0xe3 },
                ["alttp"] = new byte[] { 0xb3 },
                ["m1"] = new byte[] { 0x0B, 0xb3 },
                ["sm"] = new byte[] { 0x03, 0xEF }
            },
            ["Morph"] = new()
            {
                ["z1"] = new byte[] { 0xe4 },
                ["alttp"] = new byte[] { 0xb4 },
                ["m1"] = new byte[] { 0x0B, 0xb4 },
                ["sm"] = new byte[] { 0x23, 0xEF }
            },
            ["ScrewAttack"] = new()
            {
                ["z1"] = new byte[] { 0xe5 },
                ["alttp"] = new byte[] { 0xb5 },
                ["m1"] = new byte[] { 0x0B, 0xb5 },
                ["sm"] = new byte[] { 0x1F, 0xEF }
            },
            ["Gravity"] = new()
            {
                ["z1"] = new byte[] { 0xe6 },
                ["alttp"] = new byte[] { 0xb6 },
                ["m1"] = new byte[] { 0x0B, 0xb6 },
                ["sm"] = new byte[] { 0x0B, 0xEF }
            },
            ["HiJump"] = new()
            {
                ["z1"] = new byte[] { 0xe7 },
                ["alttp"] = new byte[] { 0xb7 },
                ["m1"] = new byte[] { 0x0B, 0xb7 },
                ["sm"] = new byte[] { 0xF3, 0xEE }
            },
            ["SpaceJump"] = new()
            {
                ["z1"] = new byte[] { 0xe8 },
                ["alttp"] = new byte[] { 0xb8 },
                ["m1"] = new byte[] { 0x0B, 0xb8 },
                ["sm"] = new byte[] { 0x1B, 0xEF }
            },
            ["Bombs"] = new()
            {
                ["z1"] = new byte[] { 0xe9 },
                ["alttp"] = new byte[] { 0xb9 },
                ["m1"] = new byte[] { 0x0B, 0xb9 },
                ["sm"] = new byte[] { 0xE7, 0xEE }
            },
            ["SpeedBooster"] = new()
            {
                ["z1"] = new byte[] { 0xea },
                ["alttp"] = new byte[] { 0xba },
                ["m1"] = new byte[] { 0x0B, 0xba },
                ["sm"] = new byte[] { 0xF7, 0xEE }
            },
            ["Charge"] = new()
            {
                ["z1"] = new byte[] { 0xeb },
                ["alttp"] = new byte[] { 0xbb },
                ["m1"] = new byte[] { 0x0B, 0xbb },
                ["sm"] = new byte[] { 0xEB, 0xEE }
            },
            ["Ice"] = new()
            {
                ["z1"] = new byte[] { 0xec },
                ["alttp"] = new byte[] { 0xbc },
                ["m1"] = new byte[] { 0x0B, 0xbc },
                ["sm"] = new byte[] { 0xEF, 0xEE }
            },
            ["Wave"] = new()
            {
                ["z1"] = new byte[] { 0xed },
                ["alttp"] = new byte[] { 0xbd },
                ["m1"] = new byte[] { 0x0B, 0xbd },
                ["sm"] = new byte[] { 0xFB, 0xEE }
            },
            ["Spazer"] = new()
            {
                ["z1"] = new byte[] { 0xee },
                ["alttp"] = new byte[] { 0xbe },
                ["m1"] = new byte[] { 0x0B, 0xbe },
                ["sm"] = new byte[] { 0xFF, 0xEE }
            },
            ["Plasma"] = new()
            {
                ["z1"] = new byte[] { 0xef },
                ["alttp"] = new byte[] { 0xbf },
                ["m1"] = new byte[] { 0x0B, 0xbf },
                ["sm"] = new byte[] { 0x13, 0xEF }
            },
            ["ETank"] = new()
            {
                ["z1"] = new byte[] { 0xf0 },
                ["alttp"] = new byte[] { 0xc0 },
                ["m1"] = new byte[] { 0x0B, 0xc0 },
                ["sm"] = new byte[] { 0xD7, 0xEE }
            },
            ["ReserveTank"] = new()
            {
                ["z1"] = new byte[] { 0xf1 },
                ["alttp"] = new byte[] { 0xc1 },
                ["m1"] = new byte[] { 0x0B, 0xc1 },
                ["sm"] = new byte[] { 0x27, 0xEF }
            },
            ["Missile"] = new()
            {
                ["z1"] = new byte[] { 0xf2 },
                ["alttp"] = new byte[] { 0xc2 },
                ["m1"] = new byte[] { 0x0B, 0xc2 },
                ["sm"] = new byte[] { 0xDB, 0xEE }
            },
            ["Super"] = new()
            {
                ["z1"] = new byte[] { 0xf3 },
                ["alttp"] = new byte[] { 0xc3 },
                ["m1"] = new byte[] { 0x0B, 0xc3 },
                ["sm"] = new byte[] { 0xDF, 0xEE }
            },
            ["PowerBomb"] = new()
            {
                ["z1"] = new byte[] { 0xf4 },
                ["alttp"] = new byte[] { 0xc4 },
                ["m1"] = new byte[] { 0x0B, 0xc4 },
                ["sm"] = new byte[] { 0xE3, 0xEE }
            },
            ["KraidToken"] = new()
            {
                ["z1"] = new byte[] { 0xf5 },
                ["alttp"] = new byte[] { 0xc5 },
                ["m1"] = new byte[] { 0x0B, 0xc5 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xc5 }
            },
            ["PhantoonToken"] = new()
            {
                ["z1"] = new byte[] { 0xf6 },
                ["alttp"] = new byte[] { 0xc6 },
                ["m1"] = new byte[] { 0x0B, 0xc6 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xc6 }
            },
            ["DraygonToken"] = new()
            {
                ["z1"] = new byte[] { 0xf7 },
                ["alttp"] = new byte[] { 0xc7 },
                ["m1"] = new byte[] { 0x0B, 0xc7 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xc7 }
            },
            ["RidleyToken"] = new()
            {
                ["z1"] = new byte[] { 0xf8 },
                ["alttp"] = new byte[] { 0xc8 },
                ["m1"] = new byte[] { 0x0B, 0xc8 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xc8 }
            },
            ["Unused1"] = new()
            {
                ["z1"] = new byte[] { 0xf9 },
                ["alttp"] = new byte[] { 0xc9 },
                ["m1"] = new byte[] { 0x0B, 0xc9 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xc9 }
            },
            ["KraidMap"] = new()
            {
                ["z1"] = new byte[] { 0xfa },
                ["alttp"] = new byte[] { 0xca },
                ["m1"] = new byte[] { 0x0B, 0xca },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xca }
            },
            ["PhantoonMap"] = new()
            {
                ["z1"] = new byte[] { 0xfb },
                ["alttp"] = new byte[] { 0xcb },
                ["m1"] = new byte[] { 0x0B, 0xcb },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xcb }
            },
            ["DraygonMap"] = new()
            {
                ["z1"] = new byte[] { 0xfc },
                ["alttp"] = new byte[] { 0xcc },
                ["m1"] = new byte[] { 0x0B, 0xcc },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xcc }
            },
            ["RidleyMap"] = new()
            {
                ["z1"] = new byte[] { 0xfd },
                ["alttp"] = new byte[] { 0xcd },
                ["m1"] = new byte[] { 0x0B, 0xcd },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xcd }
            },
            ["Unused2"] = new()
            {
                ["z1"] = new byte[] { 0xfe },
                ["alttp"] = new byte[] { 0xce },
                ["m1"] = new byte[] { 0x0B, 0xce },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xce }
            },
            ["Reserved1"] = new()
            {
                ["z1"] = new byte[] { 0xff },
                ["alttp"] = new byte[] { 0xcf },
                ["m1"] = new byte[] { 0x0B, 0xcf },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xcf }
            },

        },
        ["m1"] = new()
        {
            ["Bombs"] = new()
            {
                ["z1"] = new byte[] { 0x92 },
                ["alttp"] = new byte[] { 0x62 },
                ["m1"] = new byte[] { 0x02, 0x00 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x62 }
            },
            ["HiJump"] = new()
            {
                ["z1"] = new byte[] { 0x93 },
                ["alttp"] = new byte[] { 0x63 },
                ["m1"] = new byte[] { 0x02, 0x01 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x63 }
            },
            ["LongBeam"] = new()
            {
                ["z1"] = new byte[] { 0x96 },
                ["alttp"] = new byte[] { 0x66 },
                ["m1"] = new byte[] { 0x02, 0x02 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x66 }
            },
            ["ScrewAttack"] = new()
            {
                ["z1"] = new byte[] { 0x97 },
                ["alttp"] = new byte[] { 0x67 },
                ["m1"] = new byte[] { 0x02, 0x03 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x67 }
            },
            ["Morph"] = new()
            {
                ["z1"] = new byte[] { 0x98 },
                ["alttp"] = new byte[] { 0x68 },
                ["m1"] = new byte[] { 0x02, 0x04 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x68 }
            },
            ["Varia"] = new()
            {
                ["z1"] = new byte[] { 0x99 },
                ["alttp"] = new byte[] { 0x69 },
                ["m1"] = new byte[] { 0x02, 0x05 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x69 }
            },
            ["WaveBeam"] = new()
            {
                ["z1"] = new byte[] { 0x9c },
                ["alttp"] = new byte[] { 0x6c },
                ["m1"] = new byte[] { 0x02, 0x06 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x6c }
            },
            ["IceBeam"] = new()
            {
                ["z1"] = new byte[] { 0x9d },
                ["alttp"] = new byte[] { 0x6d },
                ["m1"] = new byte[] { 0x02, 0x07 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x6d }
            },
            ["EnergyTank"] = new()
            {
                ["z1"] = new byte[] { 0x9e },
                ["alttp"] = new byte[] { 0x6e },
                ["m1"] = new byte[] { 0x02, 0x08 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x6e }
            },
            ["Missile"] = new()
            {
                ["z1"] = new byte[] { 0x9f },
                ["alttp"] = new byte[] { 0x6f },
                ["m1"] = new byte[] { 0x02, 0x09 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0x6f }
            },

        },
        ["z1"] = new()
        {
            ["Bombs"] = new()
            {
                ["z1"] = new byte[] { 0x00 },
                ["alttp"] = new byte[] { 0xd0 },
                ["m1"] = new byte[] { 0x0B, 0xd0 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd0 }
            },
            ["SwordL1"] = new()
            {
                ["z1"] = new byte[] { 0x01 },
                ["alttp"] = new byte[] { 0xd1 },
                ["m1"] = new byte[] { 0x0B, 0xd1 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd1 }
            },
            ["SwordL2"] = new()
            {
                ["z1"] = new byte[] { 0x02 },
                ["alttp"] = new byte[] { 0xd2 },
                ["m1"] = new byte[] { 0x0B, 0xd2 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd2 }
            },
            ["SwordL3"] = new()
            {
                ["z1"] = new byte[] { 0x03 },
                ["alttp"] = new byte[] { 0xd3 },
                ["m1"] = new byte[] { 0x0B, 0xd3 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd3 }
            },
            ["Bait"] = new()
            {
                ["z1"] = new byte[] { 0x04 },
                ["alttp"] = new byte[] { 0xd4 },
                ["m1"] = new byte[] { 0x0B, 0xd4 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd4 }
            },
            ["Recorder"] = new()
            {
                ["z1"] = new byte[] { 0x05 },
                ["alttp"] = new byte[] { 0xd5 },
                ["m1"] = new byte[] { 0x0B, 0xd5 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd5 }
            },
            ["BlueCandle"] = new()
            {
                ["z1"] = new byte[] { 0x06 },
                ["alttp"] = new byte[] { 0xd6 },
                ["m1"] = new byte[] { 0x0B, 0xd6 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd6 }
            },
            ["RedCandle"] = new()
            {
                ["z1"] = new byte[] { 0x07 },
                ["alttp"] = new byte[] { 0xd7 },
                ["m1"] = new byte[] { 0x0B, 0xd7 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd7 }
            },
            ["Arrows"] = new()
            {
                ["z1"] = new byte[] { 0x08 },
                ["alttp"] = new byte[] { 0xd8 },
                ["m1"] = new byte[] { 0x0B, 0xd8 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd8 }
            },
            ["SilverArrows"] = new()
            {
                ["z1"] = new byte[] { 0x09 },
                ["alttp"] = new byte[] { 0xd9 },
                ["m1"] = new byte[] { 0x0B, 0xd9 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xd9 }
            },
            ["Bow"] = new()
            {
                ["z1"] = new byte[] { 0x0a },
                ["alttp"] = new byte[] { 0xda },
                ["m1"] = new byte[] { 0x0B, 0xda },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xda }
            },
            ["MagicalKey"] = new()
            {
                ["z1"] = new byte[] { 0x0b },
                ["alttp"] = new byte[] { 0xdb },
                ["m1"] = new byte[] { 0x0B, 0xdb },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xdb }
            },
            ["Raft"] = new()
            {
                ["z1"] = new byte[] { 0x0c },
                ["alttp"] = new byte[] { 0xdc },
                ["m1"] = new byte[] { 0x0B, 0xdc },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xdc }
            },
            ["StepLadder"] = new()
            {
                ["z1"] = new byte[] { 0x0d },
                ["alttp"] = new byte[] { 0xdd },
                ["m1"] = new byte[] { 0x0B, 0xdd },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xdd }
            },
            ["Unused1"] = new()
            {
                ["z1"] = new byte[] { 0x0e },
                ["alttp"] = new byte[] { 0xde },
                ["m1"] = new byte[] { 0x0B, 0xde },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xde }
            },
            ["Rupee5"] = new()
            {
                ["z1"] = new byte[] { 0x0f },
                ["alttp"] = new byte[] { 0xdf },
                ["m1"] = new byte[] { 0x0B, 0xdf },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xdf }
            },
            ["Rod"] = new()
            {
                ["z1"] = new byte[] { 0x10 },
                ["alttp"] = new byte[] { 0xe0 },
                ["m1"] = new byte[] { 0x0B, 0xe0 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe0 }
            },
            ["Book"] = new()
            {
                ["z1"] = new byte[] { 0x11 },
                ["alttp"] = new byte[] { 0xe1 },
                ["m1"] = new byte[] { 0x0B, 0xe1 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe1 }
            },
            ["BlueRing"] = new()
            {
                ["z1"] = new byte[] { 0x12 },
                ["alttp"] = new byte[] { 0xe2 },
                ["m1"] = new byte[] { 0x0B, 0xe2 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe2 }
            },
            ["RedRing"] = new()
            {
                ["z1"] = new byte[] { 0x13 },
                ["alttp"] = new byte[] { 0xe3 },
                ["m1"] = new byte[] { 0x0B, 0xe3 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe3 }
            },
            ["PowerBracelet"] = new()
            {
                ["z1"] = new byte[] { 0x14 },
                ["alttp"] = new byte[] { 0xe4 },
                ["m1"] = new byte[] { 0x0B, 0xe4 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe4 }
            },
            ["Letter"] = new()
            {
                ["z1"] = new byte[] { 0x15 },
                ["alttp"] = new byte[] { 0xe5 },
                ["m1"] = new byte[] { 0x0B, 0xe5 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe5 }
            },
            ["Compass"] = new()
            {
                ["z1"] = new byte[] { 0x16 },
                ["alttp"] = new byte[] { 0xe6 },
                ["m1"] = new byte[] { 0x0B, 0xe6 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe6 }
            },
            ["Map"] = new()
            {
                ["z1"] = new byte[] { 0x17 },
                ["alttp"] = new byte[] { 0xe7 },
                ["m1"] = new byte[] { 0x0B, 0xe7 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe7 }
            },
            ["Rupee"] = new()
            {
                ["z1"] = new byte[] { 0x18 },
                ["alttp"] = new byte[] { 0xe8 },
                ["m1"] = new byte[] { 0x0B, 0xe8 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe8 }
            },
            ["Key"] = new()
            {
                ["z1"] = new byte[] { 0x19 },
                ["alttp"] = new byte[] { 0xe9 },
                ["m1"] = new byte[] { 0x0B, 0xe9 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xe9 }
            },
            ["HeartContainer"] = new()
            {
                ["z1"] = new byte[] { 0x1a },
                ["alttp"] = new byte[] { 0xea },
                ["m1"] = new byte[] { 0x0B, 0xea },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xea }
            },
            ["Triforce"] = new()
            {
                ["z1"] = new byte[] { 0x1b },
                ["alttp"] = new byte[] { 0xeb },
                ["m1"] = new byte[] { 0x0B, 0xeb },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xeb }
            },
            ["MagicShield"] = new()
            {
                ["z1"] = new byte[] { 0x1c },
                ["alttp"] = new byte[] { 0xec },
                ["m1"] = new byte[] { 0x0B, 0xec },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xec }
            },
            ["Boomerang"] = new()
            {
                ["z1"] = new byte[] { 0x1d },
                ["alttp"] = new byte[] { 0xed },
                ["m1"] = new byte[] { 0x0B, 0xed },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xed }
            },
            ["MagicBoomerang"] = new()
            {
                ["z1"] = new byte[] { 0x1e },
                ["alttp"] = new byte[] { 0xee },
                ["m1"] = new byte[] { 0x0B, 0xee },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xee }
            },
            ["BluePotion"] = new()
            {
                ["z1"] = new byte[] { 0x1f },
                ["alttp"] = new byte[] { 0xef },
                ["m1"] = new byte[] { 0x0B, 0xef },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xef }
            },
            ["RedPotion"] = new()
            {
                ["z1"] = new byte[] { 0x20 },
                ["alttp"] = new byte[] { 0xf0 },
                ["m1"] = new byte[] { 0x0B, 0xf0 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xf0 }
            },
            ["Clock"] = new()
            {
                ["z1"] = new byte[] { 0x21 },
                ["alttp"] = new byte[] { 0xf1 },
                ["m1"] = new byte[] { 0x0B, 0xf1 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xf1 }
            },
            ["Heart"] = new()
            {
                ["z1"] = new byte[] { 0x22 },
                ["alttp"] = new byte[] { 0xf2 },
                ["m1"] = new byte[] { 0x0B, 0xf2 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xf2 }
            },
            ["Fairy"] = new()
            {
                ["z1"] = new byte[] { 0x23 },
                ["alttp"] = new byte[] { 0xf3 },
                ["m1"] = new byte[] { 0x0B, 0xf3 },
                ["sm"] = new byte[] { 0xE0, 0xEF, 0xf3 }
            },
        }
    };

    public static byte[]? GetItemBytes(BaseVertex location, IItem item)
    {
        string? itemFromGame = item.World.GameId;
        string? itemInGame = location.World.GameId;
        if (itemFromGame == null || itemInGame == null)
            return null;
        string itemName = item.Name;

        var itemBytes = _itemBytes.GetValueOrDefault(itemFromGame)?.GetValueOrDefault(itemName)?.GetValueOrDefault(itemInGame);
        return itemBytes;
    }

}
