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
    public int ptr { get; set; }
    public ushort room { get; set; }
    public byte elevator { get; set; }
    public byte orientation { get; set; }
    public byte x_low { get; set; }
    public byte y_low { get; set; }
    public byte x_high { get; set; }
    public byte y_high { get; set; }
    public ushort distance { get; set; }
    public ushort asm { get; set; }
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

        bytes.AddRange(BitConverter.GetBytes(door.room));
        bytes.Add(door.elevator);
        bytes.Add(door.orientation);
        bytes.Add(door.x_low);
        bytes.Add(door.y_low);
        bytes.Add(door.x_high);
        bytes.Add(door.y_high);
        bytes.AddRange(BitConverter.GetBytes(door.distance));
        bytes.AddRange(BitConverter.GetBytes(door.asm));
        return bytes.ToArray();

    }
}
