using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Randomizer.Games.SuperMetroid.Model
{
    public class MapRoomData
    {
        public required List<MapRoom> Rooms { get; set; }
    }

    public class MapRoom
    {
        public int RoomId { get; set; }
        public required string RoomName { get; set; } = string.Empty;
        public List<MapTile> MapTiles { get; set; } = new();

        [JsonInclude] public decimal? WaterLevel { get; set; }
        [JsonInclude] public bool? Heated { get; set; }

    }

    public class MapTile
    {

        private const ushort VFlip = 0x8000;
        private const ushort HFlip = 0x4000;
        private const ushort MapPalette = 0x0C00;

        // Tile coordinates: (X = Coords[0], Y = Coords[1])
        public required int[] Coords { get; set; } = Array.Empty<int>();

        // Edges
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TileEdge? Left { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TileEdge? Right { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TileEdge? Top { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TileEdge? Bottom { get; set; }

        // Interior
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TileInterior? Interior { get; set; }

        // Special
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SpecialTileType? SpecialType { get; set; }

        /// <summary>
        /// Returns a 16-bit value representing this tile's SNES tilemap entry.
        /// </summary>
        public ushort GetTileValue()
        {
            var (left, right, top, bottom) = (SubstituteEdge(Left), SubstituteEdge(Right), SubstituteEdge(Top), SubstituteEdge(Bottom));

            ushort tileMapValue = (left, right, top, bottom, Interior) switch
            {
                (_, _, _, _, TileInterior.MapStation) => 0x006F,
                (_, _, _, _, TileInterior.SaveStation) => 0x004D,
                (_, _, _, _, TileInterior.AmmoRefill) => 0x0020,
                (_, _, _, _, TileInterior.EnergyRefill) => 0x0020,
                (TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, _, _, TileInterior.ElevatorPlatformHigh) => 0x0010,
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, _, _, TileInterior.ElevatorPlatformHigh) => 0x005F,
                (TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, _, _, TileInterior.ElevatorPlatformLow) => 0x004F,
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, _, _, TileInterior.ElevatorPlatformLow) => 0x005F | VFlip,
                (TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, _) => Interior switch
                {
                    TileInterior.Item => 0x006F,
                    TileInterior.DoubleItem => 0x006F,
                    TileInterior.HiddenItem => 0x006F,
                    _ => 0x0020
                },
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, _) => Interior switch
                {
                    TileInterior.Item => 0x008F | HFlip,
                    TileInterior.DoubleItem => 0x008F | HFlip,
                    TileInterior.HiddenItem => 0x008F | HFlip,
                    _ => 0x0021 | HFlip
                },
                (TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, _) => Interior switch
                {
                    TileInterior.Item => 0x008F,
                    TileInterior.DoubleItem => 0x008F,
                    TileInterior.HiddenItem => 0x008F,
                    _ => 0x0021
                },
                (TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, _) => Interior switch
                {
                    TileInterior.Item => 0x006E | VFlip,
                    TileInterior.DoubleItem => 0x006E | VFlip,
                    TileInterior.HiddenItem => 0x006E | VFlip,
                    _ => 0x0024 | VFlip
                },
                (TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, _) => Interior switch
                {
                    TileInterior.Item => 0x006E,
                    TileInterior.DoubleItem => 0x006E,
                    TileInterior.HiddenItem => 0x006E,
                    _ => 0x0024
                },
                (TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, _) => Interior switch
                {
                    TileInterior.Item => 0x006F,
                    TileInterior.DoubleItem => 0x006F,
                    TileInterior.HiddenItem => 0x006F,
                    _ => 0x0023
                },
                (TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, _) => Interior switch
                {
                    TileInterior.Item => 0x008E,
                    TileInterior.DoubleItem => 0x008E,
                    TileInterior.HiddenItem => 0x008E,
                    _ => 0x0025           
                },
                (TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, _) => Interior switch
                {
                    TileInterior.Item => 0x008E | VFlip,
                    TileInterior.DoubleItem => 0x008E | VFlip,
                    TileInterior.HiddenItem => 0x008E | VFlip,
                    _ => 0x0025 | VFlip
                },
                (TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, _) => Interior switch
                {
                    TileInterior.Item => 0x0077,
                    TileInterior.DoubleItem => 0x0077,
                    TileInterior.HiddenItem => 0x0077,
                    _ => 0x0027 | HFlip
                },
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, _) => Interior switch
                {
                    TileInterior.Item => 0x008E | HFlip,
                    TileInterior.DoubleItem => 0x008E | HFlip,
                    TileInterior.HiddenItem => 0x008E | HFlip,
                    _ => 0x0025 | HFlip
                },
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, _) => Interior switch
                {
                    TileInterior.Item => 0x008E | HFlip | VFlip,
                    TileInterior.DoubleItem => 0x008E | HFlip | VFlip,
                    TileInterior.HiddenItem => 0x008E | HFlip | VFlip,
                    _ => 0x0025 | HFlip | VFlip
                },
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, _) => Interior switch
                {
                    TileInterior.Item => 0x0077 | HFlip,
                    TileInterior.DoubleItem => 0x0077 | HFlip,
                    TileInterior.HiddenItem => 0x0077 | HFlip,
                    _ => 0x0027
                },
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, TileEdge.Wall or TileEdge.Door, _) => Interior switch
                {
                    TileInterior.Item => 0x005E,
                    TileInterior.DoubleItem => 0x005E,
                    TileInterior.HiddenItem => 0x005E,
                    _ => 0x0022
                },
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, TileEdge.Empty or TileEdge.Passage, _) => Interior switch
                {
                    TileInterior.Item => 0x0076,
                    TileInterior.DoubleItem => 0x0076,
                    TileInterior.HiddenItem => 0x0076,
                    _ => 0x0026
                },
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, TileEdge.Wall or TileEdge.Door, _) => Interior switch
                {
                    TileInterior.Item => 0x0076 | VFlip,
                    TileInterior.DoubleItem => 0x0076 | VFlip,
                    TileInterior.HiddenItem => 0x0076 | VFlip,
                    _ => 0x0026 | VFlip
                },
                (TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, TileEdge.Empty or TileEdge.Passage, _) => Interior switch
                {
                    TileInterior.Item => 0x006F,
                    TileInterior.DoubleItem => 0x006F,
                    TileInterior.HiddenItem => 0x006F,
                    _ => 0x001B
                },
                _ => SpecialType switch
                {
                    SpecialTileType.Elevator => 0x00CE,
                    SpecialTileType.Tube => 0x006D,
                    _ => 0x001B
                }
            };

            return (ushort)(tileMapValue | MapPalette);
        }

        /// <summary>
        /// If you absolutely need a byte[] in little-endian form right here, 
        /// you can convert the ushort to two bytes: low byte then high byte.
        /// </summary>
        public byte[] GetBytes()
        {
            ushort v = GetTileValue();
            return new byte[] { (byte)(v & 0xFF), (byte)(v >> 8) };
        }

        private TileEdge? SubstituteEdge(TileEdge? edge)
        {
            return edge switch
            {
                TileEdge.QolPassage => TileEdge.Passage,
                TileEdge.QolEmpty => TileEdge.Empty,
                TileEdge.QolDoor => TileEdge.Door,
                TileEdge.QolWall => TileEdge.Wall,
                TileEdge.QolSand => TileEdge.Passage,
                TileEdge.Sand => TileEdge.Passage,
                _ => edge
            };
        }

    }

    public enum TileEdge
    {
        Wall,
        Door,
        Empty,
        Passage,
        ElevatorEntrance,
        Sand,
        QolPassage,
        QolEmpty,
        QolDoor,
        QolWall,
        QolSand
    }

    public enum TileInterior
    {
        Item,
        DoubleItem,
        HiddenItem,
        MapStation,
        SaveStation,
        AmmoRefill,
        EnergyRefill,
        DoubleRefill,
        ElevatorPlatformLow,
        ElevatorPlatformHigh,
        Event,
        Ship
    }

    public enum SpecialTileType
    {
        Elevator,
        ElevatorEntrance,
        Tube,
        SlopeUpCeilingLow,
        SlopeUpCeilingHigh,
        SlopeUpFloorLow,
        SlopeUpFloorHigh,
        SlopeDownCeilingHigh,
        SlopeDownCeilingLow,
        SlopeDownFloorHigh,
        SlopeDownFloorLow,
        Black,
        Sand
    }
}
