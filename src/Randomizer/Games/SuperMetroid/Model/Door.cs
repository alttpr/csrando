namespace Randomizer.Games.SuperMetroid.Model;

using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;


/*
 * pub struct SMDoorData {
    pub ptr: u32,
    pub room: u16,
    pub elevator: u8,
    pub orientation: u8,
    pub x_low: u8,
    pub y_low: u8,
    pub x_high: u8,
    pub y_high: u8,
    pub distance: u16,
    pub asm: u16
}
*/

public class Door
{
    [YamlMember(Alias = "ptr")]
    public int Ptr { get; set; }
    [YamlMember(Alias = "room")]
    public ushort Room { get; set; }
    [YamlMember(Alias = "elevator")]
    public byte Elevator { get; set; }
    [YamlMember(Alias = "orientation")]
    public byte Orientation { get; set; }
    [YamlMember(Alias = "x_low")]
    public byte XLow { get; set; }
    [YamlMember(Alias = "y_low")]
    public byte YLow { get; set; }
    [YamlMember(Alias = "x_high")]
    public byte XHigh { get; set; }
    [YamlMember(Alias = "y_high")]
    public byte YHigh { get; set; }
    [YamlMember(Alias = "distance")]
    public ushort Distance { get; set; }
    [YamlMember(Alias = "asm")]
    public ushort Asm { get; set; }
};

public static class DoorReader
{
    public static IEnumerable<Door> ReadDoorData()
    {
        var basePath = JsonReader.DataRoot;
        var path = System.IO.Path.Combine(basePath, "doors.yaml");

        var deserializer = new DeserializerBuilder().Build();
        try
        {
            var data = deserializer.Deserialize<List<Door>>(File.ReadAllText(path));
            return data;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while deserializing: {path}, {e.Message} ({e.InnerException?.Source ?? ""}");
            return [];
        }
    }

    /*
; Door header format
;                        _____________________________ 0: Destination room header pointer (bank $8F)
;                       |     ________________________ 2: Elevator properties
;                       |    |   _____________________ 3: Direction
;                       |    |  |   __________________ 4: Doorcap X position in blocks
;                       |    |  |  |   _______________ 5: Doorcap Y position in blocks
;                       |    |  |  |  |   ____________ 6: X screen
;                       |    |  |  |  |  |   _________ 7: Y screen
;                       |    |  |  |  |  |  |   ______ 8: Distance from door to spawn Samus
;                       |    |  |  |  |  |  |  |     _ Ah: Custom door ASM to execute (bank $8F)
;                       |    |  |  |  |  |  |  |    |
;                       rrrr ee oo xx yy XX YY dddd aaaa
    */

    public static byte[] GetDoorBytes(Door door)
    {
        var bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(door.Room));
        bytes.Add(door.Elevator);
        bytes.Add(door.Orientation);
        bytes.Add(door.XLow);
        bytes.Add(door.YLow);
        bytes.Add(door.XHigh);
        bytes.Add(door.YHigh);
        bytes.AddRange(BitConverter.GetBytes(door.Distance));
        bytes.AddRange(BitConverter.GetBytes(door.Asm));
        return bytes.ToArray();

    }
}
