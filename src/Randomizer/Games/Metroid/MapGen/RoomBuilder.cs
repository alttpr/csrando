namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using System.Linq;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>A generated cross-game portal room and the vertex its door creates.</summary>
public record PortalRoom(Point Cell, Area Area, string DoorVertexName);

/// <summary>The generated room list plus the lookups later stages need.</summary>
public class GeneratedRooms
{
    /// <summary>Generated rooms for all five playable areas (the Meta room is not included).</summary>
    public required List<Room> Rooms { get; init; }

    /// <summary>Full vertex name of the start location for graph wiring.</summary>
    public required string StartLocationName { get; init; }

    /// <summary>Coordinate to (room, screen index within the room) for diagnostics and ROM emission.</summary>
    public required Dictionary<Point, (Room Room, int Index)> CellToRoom { get; init; }

    /// <summary>Cross-game portal anchor rooms, for combo graph wiring and transition tables.</summary>
    public required List<PortalRoom> Portals { get; init; }
}

/// <summary>
/// Phase 2: decomposes the fitted grid into <see cref="YamlReader.Room"/> objects.
/// Every <see cref="Run"/> is already a maximal same-area scroll run, so runs map 1:1 to
/// rooms (caps included as screens, vanilla style). Door and elevator links are not stored
/// on rooms; <see cref="YamlReader"/>'s BuildRoom wires them spatially, which is why the
/// same grid that passed topology validation must be the one decomposed here.
/// </summary>
public static class RoomBuilder
{
    public static GeneratedRooms Build(GeneratedWorld world)
    {
        var grid = world.Grid;
        var rooms = new List<Room>();
        var cellToRoom = new Dictionary<Point, (Room, int)>();
        var portals = new List<PortalRoom>();
        string? startLocationName = null;

        // Lower areas of each elevator pair, for destination metadata on elevator sprites.
        var elevatorPairs = grid.Links.Where(l => l.Type == LinkType.Elevator)
            .ToDictionary(l => l.A, l => grid.Cell(l.B)!.Area);

        foreach (var run in grid.Runs)
        {
            var first = run.Cells[0];
            var room = new Room
            {
                name = $"{RoomNoun(run)} {first.Position.X:X2}{first.Position.Y:X2}",
                area = run.Area,
                scroll = run.Axis,
                position = [first.Position.X, first.Position.Y],
                screens = run.Cells.Select(c => c.AssignedScreen?.ScreenId
                    ?? throw new InvalidOperationException($"cell {c} has no assigned screen")).ToArray(),
                sprites = []
            };

            foreach (var (cell, index) in run.Cells.Select((c, i) => (c, i)))
            {
                cellToRoom[cell.Position] = (room, index);
                var screen = cell.AssignedScreen!;

                // Item sprites: item cells, plus boss cells whose screen carries an item
                // (Kraid's lair has the energy tank). MapStation cells stay out of this
                // set: their fixed pickup is written by RomEmitter, never by the filler.
                if (cell.Role is CellRole.Item or CellRole.Boss && screen.ItemLocationNames.Count > 0)
                {
                    room.sprites.Add(new Sprite
                    {
                        name = $"Item {cell.Position.X:X2}{cell.Position.Y:X2}",
                        screen = index,
                        location = screen.ItemLocationNames[0],
                        slot = 0x00,
                        type = SpriteType.Item,
                        // The concrete item is chosen by the item filler through the graph
                        // vertex; BuildRoom does not read this field for item sprites.
                        item = "Nothing"
                    });
                }

                // Elevator sprites with explicit destination metadata, vanilla format.
                if (cell.Role == CellRole.ElevatorTop && elevatorPairs.TryGetValue(cell.Position, out var lowerArea))
                {
                    room.sprites.Add(new Sprite
                    {
                        name = $"{cell.Area} to {lowerArea} Elevator",
                        screen = index,
                        location = screen.ElevatorLocationNames.FirstOrDefault() ?? "Elevator Platform",
                        slot = 0x00,
                        type = SpriteType.Elevator,
                        item = $"{cell.Area}To{lowerArea}"
                    });
                }

                if (cell.Role == CellRole.ElevatorBottom && screen.ElevatorLocationNames.Count > 0)
                {
                    var upperArea = grid.Links
                        .Where(l => l.Type == LinkType.Elevator && grid.Cell(l.B)!.Run == run)
                        .Select(l => grid.Cell(l.A)!.Area)
                        .FirstOrDefault(cell.Area);

                    room.sprites.Add(new Sprite
                    {
                        name = $"{cell.Area} to {upperArea} Elevator",
                        screen = index,
                        location = screen.ElevatorLocationNames[0],
                        slot = 0x00,
                        type = SpriteType.Elevator,
                        item = $"{cell.Area}To{upperArea}"
                    });
                }

                if (cell.Role == CellRole.Start && screen.StartLocationName != null)
                    startLocationName = $"{room.area} - {room.name} - {screen.Name} ({index}) - {screen.StartLocationName}";

                // Portal rooms anchor cross-game transitions through their door vertex.
                if (cell.Role == CellRole.Portal)
                {
                    var doorName = screen.LeftDoorName
                        ?? throw new InvalidOperationException($"portal screen 0x{screen.ScreenId:X2} has no left door");
                    portals.Add(new PortalRoom(cell.Position, cell.Area,
                        $"{room.area} - {room.name} - {screen.Name} ({index}) - {doorName}"));
                }
            }

            rooms.Add(room);
        }

        return new GeneratedRooms
        {
            Rooms = rooms,
            StartLocationName = startLocationName
                ?? throw new InvalidOperationException("generated world has no start location"),
            CellToRoom = cellToRoom,
            Portals = portals
        };
    }

    /// <summary>
    /// Replaces all non-Meta rooms in <paramref name="data"/> with the generated list.
    /// The Meta room must survive so existing meta/win logic keeps its vertices.
    /// </summary>
    public static void ApplyTo(YamlData data, GeneratedRooms generated)
    {
        var meta = data.rooms.Where(r => r.area == Area.Meta).ToList();
        data.rooms.Clear();
        data.rooms.AddRange(generated.Rooms);
        data.rooms.AddRange(meta);
    }

    private static string RoomNoun(Run run) => run.Cells[0].Role switch
    {
        CellRole.Boss => "Lair",
        CellRole.Gate => "Gate",
        CellRole.ElevatorTop => "Elevator Room",
        CellRole.Escape => "Escape Shaft",
        CellRole.Portal => "Portal",
        _ when run.Cells.Any(c => c.Role == CellRole.ElevatorBottom) => "Elevator Shaft",
        _ when run.Cells.Any(c => c.Role == CellRole.MapStation) => "Map Room",
        _ => run.Axis == Scrolling.Vertical ? "Shaft" : "Corridor"
    };
}
