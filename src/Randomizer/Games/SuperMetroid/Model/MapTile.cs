using System.Text.Json.Serialization;

namespace Randomizer.Games.SuperMetroid.Model
{
    public class MapRoomData
    {
        public required List<MapTileRoom> Rooms { get; set; }
    }

    public class MapTileRoom
    {
        public int RoomId { get; set; }
        public required string RoomName { get; set; } = string.Empty;
        public List<MapTile> MapTiles { get; set; } = new();

        [JsonInclude] public string? LiquidType { get; set; }
        [JsonInclude] public decimal? LiquidLevel { get; set; }
        [JsonInclude] public bool? Heated { get; set; }
    }

    public class MapTile
    {
        // ---------- SNES tile attribute bits ----------
        public const ushort VFlip = 0x8000;
        public const ushort HFlip = 0x4000;
        public const ushort Red = 0x0C00;
        public const ushort Green = 0x1000;
        public const ushort Yellow = 0x1400;
        public const ushort Orange = 0x1800;
        public const ushort Grey = 0x1C00;

        // Tile coordinates: (X = Coords[0], Y = Coords[1])
        public required int[] Coords { get; set; } = Array.Empty<int>();

        // Edges
        [JsonConverter(typeof(JsonStringEnumConverter))] public TileEdge? Left { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))] public TileEdge? Right { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))] public TileEdge? Top { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))] public TileEdge? Bottom { get; set; }

        // Interior
        [JsonConverter(typeof(JsonStringEnumConverter))] public TileInterior? Interior { get; set; }

        // Special
        [JsonConverter(typeof(JsonStringEnumConverter))] public SpecialTileType? SpecialType { get; set; }

        // ---------- Symmetry flags (mirror-only) ----------
        [Flags]
        public enum Symmetry { None = 0, H = 1, V = 2, Both = H | V }

        // ---------- Edge sets (allow union like Door|Passage, Empty|Passage, etc.) ----------
        [Flags]
        public enum EdgeSet
        {
            None = 0,
            W = 1 << 0,  // Wall
            D = 1 << 1,  // Door
            E = 1 << 2,  // Empty
            P = 1 << 3,  // Passage
            V = 1 << 4,  // ElevatorEntrance

            Any = W | D | E | P | V
        }

        // Shorthand for one-liners
        public static class E
        {
            public const EdgeSet W = EdgeSet.W;
            public const EdgeSet D = EdgeSet.D;
            public const EdgeSet Em = EdgeSet.E;   // 'E' class exists, use Em for Empty
            public const EdgeSet P = EdgeSet.P;
            public const EdgeSet V = EdgeSet.V;

            public const EdgeSet WD = W | D;
            public const EdgeSet DP = D | P;
            public const EdgeSet EP = Em | P;
            public const EdgeSet WP = W | P;
            public const EdgeSet Any = EdgeSet.Any;
        }

        // ---------- Interior grouping ----------
        private enum InteriorGroup
        {
            Default,
            Items,      // Item / DoubleItem / HiddenItem
            Refill,     // AmmoRefill / EnergyRefill / DoubleRefill
            ElevatorLow,
            ElevatorHigh,
            MapStation,
            SaveStation
        }

        private static InteriorGroup Classify(TileInterior? i) => i switch
        {
            TileInterior.Item or TileInterior.DoubleItem or TileInterior.HiddenItem => InteriorGroup.Items,
            TileInterior.AmmoRefill or TileInterior.EnergyRefill or TileInterior.DoubleRefill => InteriorGroup.Refill,
            TileInterior.ElevatorPlatformLow => InteriorGroup.ElevatorLow,
            TileInterior.ElevatorPlatformHigh => InteriorGroup.ElevatorHigh,
            TileInterior.MapStation => InteriorGroup.MapStation,
            TileInterior.SaveStation => InteriorGroup.SaveStation,
            _ => InteriorGroup.Default
        };

        [Flags]
        private enum GroupMask
        {
            None = 0,
            Default = 1 << 0,
            Items = 1 << 1,
            Refill = 1 << 2,
            ElevatorLow = 1 << 3,
            ElevatorHigh = 1 << 4,
            MapStation = 1 << 5,
            SaveStation = 1 << 6,

            All = Default | Items | Refill | ElevatorLow | ElevatorHigh | MapStation | SaveStation
        }

        private static GroupMask ToMask(InteriorGroup g) => g switch
        {
            InteriorGroup.Default => GroupMask.Default,
            InteriorGroup.Items => GroupMask.Items,
            InteriorGroup.Refill => GroupMask.Refill,
            InteriorGroup.ElevatorLow => GroupMask.ElevatorLow,
            InteriorGroup.ElevatorHigh => GroupMask.ElevatorHigh,
            InteriorGroup.MapStation => GroupMask.MapStation,
            InteriorGroup.SaveStation => GroupMask.SaveStation,
            _ => GroupMask.Default
        };

        private static EdgeSet ToSet(TileEdge? e) => Normalize(e) switch
        {
            TileEdge.Wall => EdgeSet.W,
            TileEdge.Door => EdgeSet.D,
            TileEdge.Empty => EdgeSet.E,
            TileEdge.Passage => EdgeSet.P,
            TileEdge.ElevatorEntrance => EdgeSet.V,
            _ => EdgeSet.None
        };

        private static TileEdge? Normalize(TileEdge? e) => e switch
        {
            TileEdge.QolPassage or TileEdge.QolEmpty or TileEdge.QolDoor or TileEdge.QolWall or TileEdge.QolSand => SubstituteEdge(e),
            TileEdge.Sand => TileEdge.Passage,
            _ => e
        };

        private static TileEdge? SubstituteEdge(TileEdge? edge) => edge switch
        {
            TileEdge.QolPassage => TileEdge.Passage,
            TileEdge.QolEmpty => TileEdge.Empty,
            TileEdge.QolDoor => TileEdge.Door,
            TileEdge.QolWall => TileEdge.Wall,
            TileEdge.QolSand => TileEdge.Passage,
            TileEdge.Sand => TileEdge.Passage,
            _ => edge
        };

        // ---------- TileDef + helpers ----------
        private sealed record TileDef(
            EdgeSet L, EdgeSet R, EdgeSet T, EdgeSet B,
            Symmetry Sym,
            IReadOnlyDictionary<InteriorGroup, ushort> Ids,
            GroupMask AllowedGroups,
            int Priority);

        // Build the interior ID map. Specify only what differs from Default.
        private static IReadOnlyDictionary<InteriorGroup, ushort> Ids(
            ushort @default,
            ushort? items = null,
            ushort? refill = null,
            ushort? elevLow = null,
            ushort? elevHigh = null,
            ushort? map = null,
            ushort? save = null)
        {
            var d = new Dictionary<InteriorGroup, ushort> { [InteriorGroup.Default] = @default };
            if (items is ushort i) d[InteriorGroup.Items] = i;
            if (refill is ushort r) d[InteriorGroup.Refill] = r;
            if (elevLow is ushort l) d[InteriorGroup.ElevatorLow] = l;
            if (elevHigh is ushort h) d[InteriorGroup.ElevatorHigh] = h;
            if (map is ushort m) d[InteriorGroup.MapStation] = m;
            if (save is ushort s) d[InteriorGroup.SaveStation] = s;
            return d;
        }

        // Convenience constructor (defaults AllowedGroups to All; Priority 0)
        private static TileDef Def(EdgeSet L, EdgeSet R, EdgeSet T, EdgeSet B, Symmetry sym,
                                   IReadOnlyDictionary<InteriorGroup, ushort> ids,
                                   GroupMask groups = GroupMask.All,
                                   int priority = 0)
            => new(L, R, T, B, sym, ids, groups, priority);

        // Higher priority rows run first.
        private static readonly TileDef[] TileDefs = new[]
        {
            // --- Stations: edges don't matter; match only when that interior is present ---
            Def(E.Any, E.Any, E.Any, E.Any, Symmetry.None, Ids(@default: 0x001C),
                groups: GroupMask.MapStation, priority: 1000),
            Def(E.Any, E.Any, E.Any, E.Any, Symmetry.None, Ids(@default: 0x004D),
                groups: GroupMask.SaveStation, priority: 1000),

            // --- Refill tiles ---
            // Opening on RIGHT (LEFT via H mirror); applies only to Refill group
            Def(E.W, E.D, E.Any, E.Any, Symmetry.H, Ids(@default: 0x0088, refill: 0x0088),
                groups: GroupMask.Refill, priority: 900),

            // Both sides open horizontally; Refill center
            Def(E.D, E.D, E.Any, E.Any, Symmetry.None, Ids(@default: 0x0175, refill: 0x0175),
                groups: GroupMask.Refill, priority: 900),

            // --- Elevator platforms ---
            // Base: sides open, wall bottom, open top, platform low
            Def(E.EP, E.EP, E.Any, E.W, Symmetry.None, Ids(@default: 0x005F, elevLow: 0x005F),
                groups: GroupMask.ElevatorLow, priority: 800),

            // Base: sides open, open bottom, wall top, platform high
            Def(E.EP, E.EP, E.W, E.Any, Symmetry.None, Ids(@default: 0x005F | VFlip, elevHigh: 0x005F | VFlip),
                groups: GroupMask.ElevatorHigh, priority: 800),

            // Base: sides walls, top open, elevator down, platform low
            Def(E.W, E.W, E.Any, E.EP, Symmetry.None, Ids(@default: 0x0010, elevLow: 0x0010),
                groups: GroupMask.ElevatorLow, priority: 800),

            // Base: sides walls, bottom open, elevator up, platform high
            Def(E.W, E.W, E.EP, E.Any, Symmetry.None, Ids(@default: 0x0010 | VFlip, elevHigh: 0x0010 | VFlip),
                groups: GroupMask.ElevatorHigh, priority: 800),

            // Base: walls all around, elevator entrance bottom, platform low
            Def(E.W, E.W, E.W, E.Any, Symmetry.None, Ids(@default: 0x004F, elevLow: 0x004F),
                groups: GroupMask.ElevatorLow, priority: 800),

            // Base: walls all around, elevator entrance top, platform high
            Def(E.W, E.W, E.Any, E.W, Symmetry.None, Ids(@default: 0x004F | VFlip, elevHigh: 0x004F | VFlip),
                groups: GroupMask.ElevatorHigh, priority: 800),

            // Fallback: elevator going up
            Def(E.Any, E.Any, E.V, E.Any, Symmetry.None, Ids(@default: 0x004F | VFlip),  priority: 790),

            // Fallback: elevator going down
            Def(E.Any, E.Any, E.Any, E.V, Symmetry.None, Ids(@default: 0x004F),  priority: 790),


            // -- Closed off Passages and doors -- //

            // --- Single-opening alcoves (item vs default) ---
            // Horizontal: opening on LEFT base (RIGHT via H)
            Def(E.D, E.W, E.W, E.W, Symmetry.H,
                Ids(@default: 0x0163, items: 0x0095), priority: 700),

            // Vertical: opening on TOP base (BOTTOM via V)
            Def(E.W, E.W, E.D, E.W, Symmetry.V,
                Ids(@default: 0x0164, items: 0x0098), priority: 700),

            // --- Corridors (item vs default) ---
            // Horizontal corridor: open L+R, walls T+B
            Def(E.D, E.D, E.W, E.W, Symmetry.None,
                Ids(@default: 0x0165, items: 0x009E), priority: 680),

            // Vertical corridor: open T+B, walls L+R
            Def(E.W, E.W, E.D, E.D, Symmetry.None,
                Ids(@default: 0x0166, items: 0x009F), priority: 680),

            // --- "L" shapes (four corners via H/V mirrors) ---
            // Base: open LEFT & BOTTOM; walls RIGHT & TOP
            Def(E.D, E.W, E.W, E.D, Symmetry.Both,
                Ids(@default: 0x0167, items: 0x00A5), priority: 660),

            // --- "T" shapes ---
            // Base: open LEFT & RIGHT & BOTTOM; wall on TOP (upside-down via V)
            Def(E.D, E.D, E.W, E.D, Symmetry.V,
                Ids(@default: 0x0168, items: 0x00A8), priority: 650),

            // --- "T" shapes (rotated 90 degrees) ---
            // Base: open TOP & BOTTOM & RIGHT; wall on LEFT (left-side via H)
            Def(E.W, E.D, E.D, E.D, Symmetry.H,
                Ids(@default: 0x0169, items: 0x00AE), priority: 650),

            // --- Cross shape ---
            // Base: Open all four sides
            Def(E.D, E.D, E.D, E.D, Symmetry.None,
                Ids(@default: 0x016A, items: 0x00AF), priority: 640),


            // -- Open ended Passages and doors -- //

            // -- Single-opening (item vs default) --
            // Horizontal: opening on LEFT base (RIGHT via H)
            Def(E.D, E.EP, E.EP, E.EP, Symmetry.H,
                Ids(@default: 0x014A, items: 0x00B9), priority: 600),

            // Vertical: opening on TOP base (BOTTOM via V)
            Def(E.EP, E.EP, E.D, E.EP, Symmetry.V,
                Ids(@default: 0x014B, items: 0x00BA), priority: 600),

            // -- Double-opening (item vs default) --
            // Horizontal: open LEFT & RIGHT; Empty T+B
            Def(E.D, E.D, E.EP, E.EP, Symmetry.None,
                Ids(@default: 0x015E, items: 0x0151), priority: 580),

            // Vertical: open TOP & BOTTOM; Empty L+R
            Def(E.EP, E.EP, E.D, E.D, Symmetry.None,
                Ids(@default: 0x015F, items: 0x0152), priority: 580),

            // -- Triple-opening (item vs default) --
            // Base: open LEFT & RIGHT & BOTTOM; Empty TOP, upside-down via V
            Def(E.D, E.D, E.EP, E.D, Symmetry.V,
                Ids(@default: 0x01B8, items: 0x01B9), priority: 560),

            // Base: open TOP & BOTTOM & RIGHT; Empty LEFT, left-side via H
            Def(E.EP, E.D, E.D, E.D, Symmetry.H,
                Ids(@default: 0x01BA, items: 0x01BB), priority: 560),


            // -- Open ended and wall Passages and doors -- //

            // Single-opening, wall on one side (item vs default)
            // Base: open LEFT, wall TOP, empty RIGHT & BOTTOM (RIGHT via H, BOTTOM via V, LEFT+BOTTOM via H+V)
            Def(E.D, E.EP, E.W, E.EP, Symmetry.Both,
                Ids(@default: 0x014D, items: 0x00CC), priority: 500),

            // Base: open TOP, wall RIGHT, empty LEFT & BOTTOM (LEFT via H, TOP via V, LEFT+TOP via H+V)
            Def(E.EP, E.W, E.D, E.EP, Symmetry.Both,
                Ids(@default: 0x014E, items: 0x00CD), priority: 500),

            // Base: open LEFT, wall RIGHT, empty TOP & BOTTOM (RIGHT via H)
            Def(E.D, E.W, E.EP, E.EP, Symmetry.H,
                Ids(@default: 0x01F4, items: 0x01F5), priority: 490),

            // Base: open TOP, wall BOTTOM, empty LEFT & RIGHT (BOTTOM via V)
            Def(E.EP, E.EP, E.D, E.W, Symmetry.V,
                Ids(@default: 0x01F6, items: 0x01F7), priority: 490),

            // Base: open LEFT, wall TOP and RIGHT, empty BOTTOM (RIGHT via H, BOTTOM via V, LEFT+BOTTOM via H+V)
            Def(E.D, E.W, E.W, E.EP, Symmetry.Both,
                Ids(@default: 0x01D8, items: 0x01D9), priority: 485),

            // Base: open TOP, wall RIGHT and BOTTOM, empty LEFT (LEFT via H, TOP via V, LEFT+TOP via H+V)
            Def(E.EP, E.W, E.D, E.W, Symmetry.Both,
                Ids(@default: 0x01E8, items: 0x01F8), priority: 482),

            // Double-opening, wall on one side (item vs default)
            // Base: open LEFT & RIGHT, wall TOP, empty BOTTOM (upside-down via V)
            Def(E.D, E.D, E.W, E.EP, Symmetry.V,
                Ids(@default: 0x014F, items: 0x0144), priority: 480),

            // Base: open TOP & LEFT, empty RIGHT & BOTTOM (RIGHT via H, BOTTOM via V, LEFT+BOTTOM via H+V)
            Def(E.D, E.EP, E.D, E.EP, Symmetry.Both,
                Ids(@default: 0x0050, items: 0x0051), priority: 475),

            // Base: open TOP & LEFT, empty RIGHT, wall BOTTOM (RIGHT via H, BOTTOM via V, LEFT+BOTTOM via H+V)
            Def(E.D, E.EP, E.D, E.W, Symmetry.Both,
                Ids(@default: 0x0052, items: 0x0053), priority: 470),

            // Base: open TOP & LEFT, wall RIGHT, empty BOTTOM (RIGHT via H, BOTTOM via V, LEFT+BOTTOM via H+V)
            Def(E.D, E.W, E.D, E.EP, Symmetry.Both,
                Ids(@default: 0x0054, items: 0x0055), priority: 465),

            // Single-opening, wall on two sides (item vs default)
            // Base: open LEFT, walls TOP & BOTTOM, empty RIGHT (RIGHT via H)
            Def(E.D, E.EP, E.W, E.W, Symmetry.H,
                Ids(@default: 0x015A, items: 0x016B), priority: 460),

            // Single-opening, wall on two sides (item vs default)
            // Base: open TOP, walls RIGHT & LEFT, empty BOTTOM (BOTTOM via V)
            Def(E.W, E.W, E.D, E.EP, Symmetry.V,
                Ids(@default: 0x015B, items: 0x016D), priority: 460),


            // -- Walls and empty tiles -- //
            // All walls (item vs default)
            Def(E.W, E.W, E.W, E.W, Symmetry.None,
                Ids(@default: 0x0020, items: 0x006F), priority: 400),

            // All empty (item vs default)
            Def(E.EP, E.EP, E.EP, E.EP, Symmetry.None,
                Ids(@default: 0x001B, items: 0x006F), priority: 400), // TODO: change item ID here with a proper tile?

            // Single wall on RIGHT (item vs default) (LEFT via H)
            Def(E.EP, E.W, E.EP, E.EP, Symmetry.H,
                Ids(@default: 0x0027, items: 0x01BF), priority: 380),

            // Single wall on TOP (item vs default) (BOTTOM via V)
            Def(E.EP, E.EP, E.W, E.EP, Symmetry.V,
                Ids(@default: 0x0026, items: 0x0076), priority: 380),

            // Single wall on LEFT & TOP (item vs default) (RIGHT via H, BOTTOM via V, LEFT+BOTTOM via H+V)
            Def(E.W, E.EP, E.W, E.EP, Symmetry.Both,
                Ids(@default: 0x0025, items: 0x008E), priority: 360),

            // "U" shape: walls on three sides, empty on one side (item vs default)
            // Base: walls LEFT, TOP, RIGHT; empty BOTTOM (upside-down via V)
            Def(E.W, E.W, E.W, E.EP, Symmetry.V,
                Ids(@default: 0x0024, items: 0x006E), priority: 340),

            // "U" shape: walls on three sides, empty on one side (item vs default)
            // Base: walls TOP, LEFT, BOTTOM; empty RIGHT (RIGHT via H)
            Def(E.W, E.EP, E.W, E.W, Symmetry.H,
                Ids(@default: 0x0021, items: 0x008F), priority: 340),

            // Double walls on opposite sides (item vs default)
            // Base: walls LEFT & RIGHT; empty TOP & BOTTOM
            Def(E.W, E.W, E.EP, E.EP, Symmetry.None,
                Ids(@default: 0x0023, items: 0x01CF), priority: 320),

            // Base: walls TOP & BOTTOM; empty LEFT & RIGHT
            Def(E.EP, E.EP, E.W, E.W, Symmetry.None,
                Ids(@default: 0x0022, items: 0x005E), priority: 320),

        };

        // ---------- Matching engine (mirror only; no rotation) ----------
        private static bool Matches(EdgeSet rule, EdgeSet actual) => (rule & actual) != 0;

        private static bool TryMatchDef(
            TileDef d,
            (EdgeSet L, EdgeSet R, EdgeSet T, EdgeSet B) a,
            InteriorGroup g,
            out ushort tileValue)
        {
            tileValue = 0;
            var gMask = ToMask(g);
            if ((d.AllowedGroups & gMask) == 0) return false;

            // Identity
            if (Matches(d.L, a.L) && Matches(d.R, a.R) && Matches(d.T, a.T) && Matches(d.B, a.B))
                return Resolve(d, g, 0, out tileValue);

            // H mirror
            if (d.Sym.HasFlag(Symmetry.H) &&
                Matches(d.L, a.R) && Matches(d.R, a.L) && Matches(d.T, a.T) && Matches(d.B, a.B))
                return Resolve(d, g, HFlip, out tileValue);

            // V mirror
            if (d.Sym.HasFlag(Symmetry.V) &&
                Matches(d.L, a.L) && Matches(d.R, a.R) && Matches(d.T, a.B) && Matches(d.B, a.T))
                return Resolve(d, g, VFlip, out tileValue);

            // H+V
            if (d.Sym.HasFlag(Symmetry.H) && d.Sym.HasFlag(Symmetry.V) &&
                Matches(d.L, a.R) && Matches(d.R, a.L) && Matches(d.T, a.B) && Matches(d.B, a.T))
                return Resolve(d, g, (ushort)(HFlip | VFlip), out tileValue);

            return false;
        }

        private static bool Resolve(TileDef d, InteriorGroup g, ushort flips, out ushort tileValue)
        {
            var baseId = d.Ids.TryGetValue(g, out var v) ? v : d.Ids[InteriorGroup.Default];
            tileValue = (ushort)(baseId | flips);
            return true;
        }


        private static bool TryGetSlopeTileValue(SpecialTileType t, out ushort value)
        {
            value = t switch
            {
                SpecialTileType.SlopeDownFloorHigh => 0x0015,
                SpecialTileType.SlopeDownFloorLow => 0x0016,
                SpecialTileType.SlopeDownCeilingHigh => 0x0013,
                SpecialTileType.SlopeDownCeilingLow => 0x0014,
                SpecialTileType.SlopeUpFloorHigh => 0x0015 | HFlip,
                SpecialTileType.SlopeUpFloorLow => 0x0016 | HFlip,
                SpecialTileType.SlopeUpCeilingHigh => 0x0013 | HFlip,
                SpecialTileType.SlopeUpCeilingLow => 0x0014 | HFlip,
                _ => 0
            };
           
            return value != 0;
        }


        /// <summary>
        /// Returns a 16-bit value representing this tile's SNES tilemap entry.
        /// </summary>
        public ushort GetTileValue()
        {
            // Normalize edges into sets
            var a = (ToSet(Left), ToSet(Right), ToSet(Top), ToSet(Bottom));
            var g = Classify(Interior);

            if(TryGetSlopeTileValue(SpecialType ?? 0, out var slopeVal))
                return slopeVal;

            // Highest priority first
            foreach (var d in TileDefs.OrderByDescending(t => t.Priority))
            {
                if (TryMatchDef(d, a, g, out var val))
                    return val;
            }

            // Special fallbacks
            if (SpecialType is SpecialTileType.Black) return 0x001F;
            if (SpecialType is SpecialTileType.Elevator) return 0x00CE;
            if (SpecialType is SpecialTileType.Tube)
            {
                return 0x00CE;
            }

            // Default empty
            Console.WriteLine("Warning: could not match tile at " +
                $"({Coords.ElementAtOrDefault(0)}, {Coords.ElementAtOrDefault(1)}) " +
                $"with edges L={Left}, R={Right}, T={Top}, B={Bottom} and interior {Interior}. " +
                "Using empty tile as fallback.");
            return 0x001B;
        }

        /// <summary>
        /// Returns the map tile for a station room converted to host a portal. Non-station
        /// tiles retain their normal map tile.
        /// </summary>
        public ushort GetPortalTileValue(bool portalOnLeft) => Interior switch
        {
            TileInterior.MapStation => (ushort)(0x001D | (portalOnLeft ? 0 : HFlip)),
            TileInterior.AmmoRefill or TileInterior.EnergyRefill or TileInterior.DoubleRefill =>
                (ushort)(0x001E | (portalOnLeft ? 0 : HFlip)),
            TileInterior.SaveStation => portalOnLeft ? (ushort)0x0176 : (ushort)0x0177,
            _ => GetTileValue(),
        };

        /// <summary>
        /// If you absolutely need a byte[] in little-endian form right here,
        /// you can convert the ushort to two bytes: low byte then high byte.
        /// </summary>
        public byte[] GetBytes()
        {
            ushort v = GetTileValue();
            return new byte[] { (byte)(v & 0xFF), (byte)(v >> 8) };
        }

        public byte[] GetPortalBytes(bool portalOnLeft)
        {
            ushort v = GetPortalTileValue(portalOnLeft);
            return new byte[] { (byte)(v & 0xFF), (byte)(v >> 8) };
        }
    }

    public enum TileEdge
    {
        Wall, Door, Empty, Passage, ElevatorEntrance, Sand,
        QolPassage, QolEmpty, QolDoor, QolWall, QolSand
    }

    public enum TileInterior
    {
        Item, DoubleItem, HiddenItem,
        MapStation, SaveStation,
        AmmoRefill, EnergyRefill, DoubleRefill,
        ElevatorPlatformLow, ElevatorPlatformHigh,
        Event, Ship
    }

    public enum SpecialTileType
    {
        Elevator, ElevatorEntrance, Tube,
        SlopeUpCeilingLow, SlopeUpCeilingHigh,
        SlopeUpFloorLow, SlopeUpFloorHigh,
        SlopeDownCeilingHigh, SlopeDownCeilingLow,
        SlopeDownFloorHigh, SlopeDownFloorLow,
        Black, Sand
    }
}
