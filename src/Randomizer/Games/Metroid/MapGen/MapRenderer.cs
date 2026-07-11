namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// Renders a generated world layout for human inspection: an SVG image with area colors,
/// run outlines, doors, elevators and role markers, and a compact ASCII fallback.
/// </summary>
public static class MapRenderer
{
    private const int CellSize = 28;
    private const int Margin = 40;

    private static readonly Dictionary<Area, string> AreaColors = new()
    {
        [Area.Brinstar] = "#4a7abf",
        [Area.Norfair] = "#bf6a4a",
        [Area.Kraid] = "#5fa05f",
        [Area.Ridley] = "#a05fa0",
        [Area.Tourian] = "#b8b85a",
    };

    private static readonly Dictionary<CellRole, string> RoleMarks = new()
    {
        [CellRole.Start] = "S",
        [CellRole.Boss] = "B",
        [CellRole.Item] = "i",
        [CellRole.Gate] = "G",
        [CellRole.ElevatorTop] = "E",
        [CellRole.ElevatorBottom] = "e",
        [CellRole.Cap] = "#",
        [CellRole.Escape] = "x",
        [CellRole.Portal] = "P",
    };

    public static string ToSvg(GeneratedWorld world)
    {
        var grid = world.Grid;
        int size = WorldGrid.Size * CellSize + 2 * Margin;
        var svg = new StringBuilder();
        svg.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{size}\" height=\"{size}\" font-family=\"monospace\">");
        svg.AppendLine($"<rect width=\"{size}\" height=\"{size}\" fill=\"#181818\"/>");

        // Grid lines and coordinate labels.
        for (int i = 0; i <= WorldGrid.Size; i++)
        {
            int p = Margin + i * CellSize;
            svg.AppendLine($"<line x1=\"{p}\" y1=\"{Margin}\" x2=\"{p}\" y2=\"{size - Margin}\" stroke=\"#282828\" stroke-width=\"1\"/>");
            svg.AppendLine($"<line x1=\"{Margin}\" y1=\"{p}\" x2=\"{size - Margin}\" y2=\"{p}\" stroke=\"#282828\" stroke-width=\"1\"/>");
            if (i < WorldGrid.Size && i % 2 == 0)
            {
                svg.AppendLine($"<text x=\"{Margin + i * CellSize + CellSize / 2}\" y=\"{Margin - 8}\" fill=\"#888\" font-size=\"9\" text-anchor=\"middle\">{i:X}</text>");
                svg.AppendLine($"<text x=\"{Margin - 8}\" y=\"{Margin + i * CellSize + CellSize / 2 + 3}\" fill=\"#888\" font-size=\"9\" text-anchor=\"end\">{i:X}</text>");
            }
        }

        // Cells.
        foreach (var cell in grid.Cells)
        {
            int x = Margin + cell.Position.X * CellSize;
            int y = Margin + cell.Position.Y * CellSize;
            string color = AreaColors.GetValueOrDefault(cell.Area, "#777777");
            string fill = cell.Role == CellRole.Cap ? "#333333" : color;
            svg.AppendLine($"<rect x=\"{x + 1}\" y=\"{y + 1}\" width=\"{CellSize - 2}\" height=\"{CellSize - 2}\" fill=\"{fill}\" opacity=\"0.85\"/>");

            // Walls: thick edge lines where the cell requires a wall.
            foreach (var dir in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
            {
                var (x1, y1, x2, y2) = dir switch
                {
                    Direction.Up => (x, y, x + CellSize, y),
                    Direction.Down => (x, y + CellSize, x + CellSize, y + CellSize),
                    Direction.Left => (x, y, x, y + CellSize),
                    _ => (x + CellSize, y, x + CellSize, y + CellSize),
                };

                switch (cell.Edge(dir))
                {
                    case EdgeRequirement.Wall:
                        svg.AppendLine($"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"#000\" stroke-width=\"3\"/>");
                        break;
                    case EdgeRequirement.Door:
                        var doorColor = (dir == Direction.Left ? cell.LeftDoorColor : cell.RightDoorColor) switch
                        {
                            DoorType.Red => "#ff5555",
                            DoorType.Purple => "#cc66ff",
                            DoorType.Orange => "#ffaa33",
                            _ => "#66ccff",
                        };
                        int dx = dir == Direction.Left ? x : x + CellSize;
                        svg.AppendLine($"<rect x=\"{dx - 3}\" y=\"{y + CellSize / 2 - 6}\" width=\"6\" height=\"12\" fill=\"{doorColor}\"/>");
                        break;
                    case EdgeRequirement.Elevator:
                        int ey = dir == Direction.Up ? y : y + CellSize;
                        svg.AppendLine($"<polygon points=\"{x + CellSize / 2 - 6},{ey} {x + CellSize / 2 + 6},{ey} {x + CellSize / 2},{ey + (dir == Direction.Up ? -7 : 7)}\" fill=\"#ffffff\"/>");
                        break;
                }
            }

            // Role marker and forced screen id.
            if (RoleMarks.TryGetValue(cell.Role, out var mark))
                svg.AppendLine($"<text x=\"{x + CellSize / 2}\" y=\"{y + CellSize / 2 + 4}\" fill=\"#fff\" font-size=\"13\" font-weight=\"bold\" text-anchor=\"middle\">{mark}</text>");
            else if (cell.ForcedScreenId.HasValue)
                svg.AppendLine($"<text x=\"{x + CellSize / 2}\" y=\"{y + CellSize / 2 + 3}\" fill=\"#ddd\" font-size=\"8\" text-anchor=\"middle\">{cell.ForcedScreenId:X2}</text>");
        }

        // Legend with landmarks and diagnostics.
        int ly = size - Margin + 14;
        svg.AppendLine($"<text x=\"{Margin}\" y=\"{ly}\" fill=\"#aaa\" font-size=\"11\">seed {world.Seed}  " +
            string.Join("  ", world.Landmarks.Select(kv => $"{kv.Key}={kv.Value}")) + "</text>");

        int lx = Margin;
        foreach (var (area, color) in AreaColors)
        {
            svg.AppendLine($"<rect x=\"{lx}\" y=\"{ly + 8}\" width=\"10\" height=\"10\" fill=\"{color}\"/>");
            svg.AppendLine($"<text x=\"{lx + 14}\" y=\"{ly + 17}\" fill=\"#aaa\" font-size=\"11\">{area}</text>");
            lx += 90;
        }

        svg.AppendLine("</svg>");
        return svg.ToString();
    }

    /// <summary>
    /// Compact text view: one 3-character block per cell (area letter, role/axis mark,
    /// door/elevator hint), suitable for console/test output.
    /// </summary>
    public static string ToAscii(GeneratedWorld world)
    {
        var grid = world.Grid;
        var sb = new StringBuilder();
        sb.AppendLine("    " + string.Join("", Enumerable.Range(0, WorldGrid.Size).Select(x => (x % 16).ToString("X").PadLeft(3))));

        for (int y = 0; y < WorldGrid.Size; y++)
        {
            sb.Append($"{y:X2}  ");
            for (int x = 0; x < WorldGrid.Size; x++)
            {
                var cell = grid.Cell(x, y);
                if (cell == null)
                {
                    sb.Append(grid.ReservedEmpty.Contains(new Point(x, y)) ? " . " : "   ");
                    continue;
                }

                char areaChar = cell.Area switch
                {
                    Area.Brinstar => 'B',
                    Area.Norfair => 'N',
                    Area.Kraid => 'K',
                    Area.Ridley => 'R',
                    Area.Tourian => 'T',
                    _ => '?'
                };

                char mid = RoleMarks.TryGetValue(cell.Role, out var mark) ? mark[0]
                    : cell.Axis == Scrolling.Vertical ? '|' : '-';

                char right = cell.Right switch
                {
                    EdgeRequirement.Door => 'd',
                    EdgeRequirement.Scroll => '.',
                    _ => ' '
                };

                sb.Append(areaChar).Append(mid).Append(right);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
