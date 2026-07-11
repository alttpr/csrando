namespace Randomizer.Games.Metroid;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Randomizer.Games.Metroid.MapGen;
using Randomizer.Graph;
using BaseVertex = Randomizer.Graph.Vertex;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>One cell of the generated map, shaped for the web spoiler map viewer.</summary>
public sealed class MapSpoilerCell
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required string Area { get; init; }
    public string? Role { get; init; }
    public required string Screen { get; init; }
    /// <summary>Non-wall edges: direction ("up"/"down"/"left"/"right") to "Scroll",
    /// "Elevator" or "Door:&lt;color&gt;".</summary>
    public required Dictionary<string, string> Edges { get; init; }
}

public sealed class MapSpoilerData
{
    public required List<MapSpoilerCell> Cells { get; init; }
    /// <summary>"x,y" to the item placed on that cell.</summary>
    public required Dictionary<string, string> ItemPlacements { get; init; }
    /// <summary>Landmark name (Start, MotherBrain, Portal0, ...) to "x,y".</summary>
    public required Dictionary<string, string> Landmarks { get; init; }
}

public static class Spoiler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Appends the generated map (when map shuffle is on) as the "m1Map" spoiler section,
    /// rendered by the website's map viewer. <paramref name="itemName"/> lets the combo
    /// spoiler tag items with their game.
    /// </summary>
    public static void AppendMap(
        Dictionary<string, Dictionary<string, string>> spoiler,
        World world,
        Func<IItem, string>? itemName = null)
    {
        var generated = world.GeneratedMap;
        if (generated == null)
            return;

        var data = new MapSpoilerData
        {
            Cells = generated.Grid.Cells.Select(BuildCell).ToList(),
            ItemPlacements = BuildItemPlacements(world, generated, itemName ?? (item => item.Name)),
            Landmarks = new Dictionary<string, string>(
                generated.Landmarks.Select(kv =>
                    new KeyValuePair<string, string>(kv.Key, $"{kv.Value.X},{kv.Value.Y}")))
            {
                ["Start"] = $"{generated.Start.X},{generated.Start.Y}"
            },
        };

        spoiler["m1Map"] = new Dictionary<string, string> { ["data"] = JsonSerializer.Serialize(data, JsonOptions) };
    }

    private static MapSpoilerCell BuildCell(AbstractCell cell)
    {
        var edges = new Dictionary<string, string>();
        foreach (var dir in Directions.All)
        {
            var edge = cell.Edge(dir);
            if (edge == EdgeRequirement.Wall)
                continue;
            var color = dir == Direction.Left ? cell.LeftDoorColor : cell.RightDoorColor;
            edges[dir.ToString().ToLowerInvariant()] =
                edge == EdgeRequirement.Door ? $"Door:{color ?? DoorType.Blue}" : edge.ToString();
        }

        return new MapSpoilerCell
        {
            X = cell.Position.X,
            Y = cell.Position.Y,
            Area = cell.Area.ToString(),
            Role = cell.Role is CellRole.Shaft or CellRole.Corridor ? null : cell.Role.ToString(),
            Screen = $"{cell.AssignedScreen?.ScreenId ?? 0:X2}",
            Edges = edges,
        };
    }

    /// <summary>
    /// Locates each filled item on the grid through its emitted ROM address (every item
    /// vertex's address maps 1:1 to the cell whose table entry it overwrites).
    /// </summary>
    private static Dictionary<string, string> BuildItemPlacements(
        World world, GeneratedWorld generated, Func<IItem, string> itemName)
    {
        var placements = new Dictionary<string, string>();
        if (generated.ItemAddresses == null)
            return placements;

        var cellByAddress = generated.ItemAddresses.ToDictionary(kv => (long)kv.Value, kv => kv.Key);
        foreach (var location in world.GetLocationsOfType(VertexType.Item).OfType<BaseVertex>())
        {
            if (location.Item == null || location.Addresses is not [var address, ..])
                continue;
            if (cellByAddress.TryGetValue(address, out var cell))
                placements[$"{cell.X},{cell.Y}"] = itemName(location.Item);
        }

        return placements;
    }
}
